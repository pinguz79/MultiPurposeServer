using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace MultiPurposeServer.Client
{
    /// <summary>Trasporto di lettura senza retry o politiche di navigazione. Il chiamante gestisce il lifecycle di HttpClient.</summary>
    public sealed class ApiClient(HttpClient http, Uri? server = null,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        /// <summary>Legge una risposta JSON con metadati espliciti compatibili con trimming, propagando la cancellazione.</summary>
        /// <exception cref="ApiException">Risposta HTTP non riuscita, nulla o non interpretabile.</exception>
        public async Task<T> Get<T>(string path, JsonTypeInfo<T> jsonType, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, server is null ? new Uri(path, UriKind.Relative) : new Uri(server, path));
            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string message = $"Il server ha restituito un errore HTTP {(int)response.StatusCode}.";
                try
                {
                    using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                    if (problem.RootElement.ValueKind == JsonValueKind.Object && problem.RootElement.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                    {
                        message = detail.GetString() ?? message;
                    }
                }
                catch (JsonException)
                {
                    // Non mostrare all'utente il contenuto di eventuali pagine HTML di errore.
                }

                throw new ApiException(response.StatusCode, message);
            }

            try
            {
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync(stream, jsonType, cancellationToken) ?? throw new ApiException(response.StatusCode, "Il server ha restituito una risposta vuota.");
            }
            catch (JsonException)
            {
                throw new ApiException(response.StatusCode, "Il server ha restituito dati non validi.");
            }
        }

        /// <summary>Valida l'indirizzo HTTPS di base, mantenendo l'eventuale percorso e aggiungendo lo slash finale.</summary>
        /// <exception cref="ArgumentException">Indirizzo non HTTPS o contenente credenziali, query o frammenti.</exception>
        public static Uri ValidateServer(string address) => !Uri.TryCreate(address.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ? throw new ArgumentException("Inserisci un indirizzo HTTPS del server, senza credenziali, query o frammenti.", nameof(address)) : new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
    }
}
