using Microsoft.Extensions.Options;
using Portfolio.Api.Services.Models;
using Portfolio.Api.Services.Options;
using System.Text;
using System.Text.Json;

namespace Portfolio.Api.Services;

public class CacheService(HttpClient httpClient, IOptions<PortfolioCacheOptions> options) : ICacheService
{
    private readonly PortfolioCacheOptions _options = options.Value;

    public async Task<CacheClearOperationResult> Clear(bool clearAlbumRoutingCache, bool clearPhotoRoutingCache, bool clearApiResponseCache)
    {
        var request = new CacheClearOperationRequest(clearAlbumRoutingCache, clearPhotoRoutingCache, clearApiResponseCache);

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        using var message = new HttpRequestMessage(HttpMethod.Post, _options.ClearEndpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        message.Headers.Add("X-Portfolio-Shared-Secret", _options.SharedSecret);

        using var response = await httpClient.SendAsync(message);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Portfolio.Web cache clear failed with status {(int)response.StatusCode}: {responseBody}");

        try
        {
            return JsonSerializer.Deserialize<CacheClearOperationResult>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Portfolio.Web returned an empty cache clear response.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Portfolio.Web returned invalid JSON: {responseBody}", exception);
        }
    }
}