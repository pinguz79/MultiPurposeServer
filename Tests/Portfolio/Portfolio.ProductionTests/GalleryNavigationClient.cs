using System.Net.Http.Json;
using System.Text.Json;

using Portfolio.ProductionTests.Models;

namespace Portfolio.ProductionTests
{
    internal sealed class GalleryNavigationClient(ProductionTestSettings settings) : IDisposable
    {
        private const string ApiKeyHeader = "X-Portfolio-Api-Key";
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _apiClient = CreateApiClient(settings);
        private readonly HttpClient _webClient = new() { BaseAddress = settings.WebBaseUrl, Timeout = TimeSpan.FromSeconds(30) };


        #region Navigazione

        public async Task<NavigationRun> Browse(string phase, CancellationToken cancellationToken = default)
        {
            var run = new NavigationRun(phase);
            var visitedAlbumIds = new HashSet<Guid>();

            await CheckWebPage(string.Empty, "Portfolio.Web root", run, cancellationToken);
            await BrowseChildren(null, visitedAlbumIds, run, cancellationToken);

            return run;
        }

        public async Task<CacheClearResult> ClearAllCaches(CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "BackEnd/Cache/Clear");
            request.Headers.Add(ApiKeyHeader, settings.BackEndApiKey);
            request.Content = JsonContent.Create(new
            {
                clearAlbumRoutingCache = true,
                clearPhotoRoutingCache = true,
                clearApiResponseCache = true
            });

            using var response = await _apiClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            return !response.IsSuccessStatusCode ? throw new InvalidOperationException($"Cache clear returned HTTP {(int)response.StatusCode}: {responseBody}")
                : JsonSerializer.Deserialize<CacheClearResult>(responseBody, JsonOptions) ?? throw new InvalidOperationException("Cache clear returned an empty or invalid response.");
        }

        public void Dispose()
        {
            _apiClient.Dispose();
            _webClient.Dispose();
        }

        private async Task<int?> BrowseChildren(Guid? parentId, HashSet<Guid> visitedAlbumIds, NavigationRun run, CancellationToken cancellationToken)
        {
            var endpoint = parentId.HasValue
                ? $"FrontEnd/Home/Albums?id={Uri.EscapeDataString(parentId.Value.ToString())}"
                : "FrontEnd/Home/Albums";

            var albums = await GetJson<List<AlbumResponse>>(endpoint, run, cancellationToken);

            if (albums is null)
            {
                return null;
            }

            foreach (var album in albums)
            {
                if (!visitedAlbumIds.Add(album.Id))
                {
                    run.AddFailure("Hierarchy", album.FullPath ?? album.Path ?? album.Id.ToString(), "Album ID encountered more than once.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(album.FullPath))
                {
                    run.AddFailure("API contract", album.Id.ToString(), "A non-null fullPath is required for navigation.");
                    continue;
                }

                await CheckRouteResolution(album, run, cancellationToken);
                await CheckWebPage(EncodePath(album.FullPath), album.FullPath, run, cancellationToken);

                var discoveredChildren = await BrowseChildren(album.Id, visitedAlbumIds, run, cancellationToken);

                if (discoveredChildren.HasValue && discoveredChildren.Value != album.Children)
                {
                    run.AddFailure(
                        "API hierarchy",
                        album.FullPath,
                        $"The DTO declares {album.Children} child album(s), but the child endpoint returned {discoveredChildren.Value}.");
                }

                if (discoveredChildren == 0)
                {
                    await CheckPhotoPages(album, run, cancellationToken);
                }
            }

            return albums.Count;
        }

        #endregion

        #region Verifica API

        private async Task CheckRouteResolution(AlbumResponse album, NavigationRun run, CancellationToken cancellationToken)
        {
            var endpoint = $"FrontEnd/Routing/Album?path={Uri.EscapeDataString(album.FullPath!)}";
            var resolved = await GetJson<AlbumResponse>(endpoint, run, cancellationToken);

            if (resolved is null)
            {
                return;
            }

            if (resolved.Id != album.Id || !string.Equals(resolved.FullPath, album.FullPath, StringComparison.Ordinal))
            {
                run.AddFailure(
                    "API routing",
                    album.FullPath!,
                    $"Expected {album.Id} / {album.FullPath}, received {resolved.Id} / {resolved.FullPath}.");
            }
        }

        private async Task CheckPhotoPages(AlbumResponse album, NavigationRun run, CancellationToken cancellationToken)
        {
            var firstEndpoint = $"FrontEnd/Home/Album/{album.Id}/Photos?page=1&pageSize=48";
            var firstPage = await GetJson<PhotoPageResponse>(firstEndpoint, run, cancellationToken);

            if (firstPage is null)
            {
                return;
            }

            for (var page = 2; page <= firstPage.TotalPages; page++)
            {
                await GetJson<PhotoPageResponse>($"FrontEnd/Home/Album/{album.Id}/Photos?page={page}&pageSize=48", run, cancellationToken);
            }
        }

        private async Task<T?> GetJson<T>(string endpoint, NavigationRun run, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add(ApiKeyHeader, settings.FrontEndApiKey);

            try
            {
                using var response = await _apiClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    run.AddFailure("Portfolio.Api", endpoint, $"HTTP {(int)response.StatusCode}: {Truncate(responseBody)}");
                    return default;
                }

                try
                {
                    return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
                }
                catch (JsonException exception)
                {
                    run.AddFailure("Portfolio.Api", endpoint, $"Invalid JSON: {exception.Message}");
                    return default;
                }
            }
            catch (HttpRequestException exception)
            {
                run.AddFailure("Portfolio.Api", endpoint, exception.Message);
                return default;
            }
            catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                run.AddFailure("Portfolio.Api", endpoint, $"Request timed out: {exception.Message}");
                return default;
            }
        }

        #endregion

        #region Verifica Web

        private async Task CheckWebPage(string relativeUrl, string label, NavigationRun run, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _webClient.GetAsync(relativeUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    run.AddFailure("Portfolio.Web", label, $"HTTP {(int)response.StatusCode}: {Truncate(responseBody)}");
                }
            }
            catch (HttpRequestException exception)
            {
                run.AddFailure("Portfolio.Web", label, exception.Message);
            }
            catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                run.AddFailure("Portfolio.Web", label, $"Request timed out: {exception.Message}");
            }
        }

        private static HttpClient CreateApiClient(ProductionTestSettings settings) => new()
        {
            BaseAddress = settings.ApiBaseUrl,
            Timeout = TimeSpan.FromSeconds(30)
        };

        #endregion

        #region Formattazione diagnostica

        private static string EncodePath(string path) => string.Join('/', path.Split('/').Select(Uri.EscapeDataString));

        private static string Truncate(string value) => value.Length <= 300 ? value : value[..300] + "â€¦";

        #endregion

    }
}
