using MultiPurposeServer.Client;

namespace Finance.Client
{
    /// <summary>Crea client Finance con configurazione per richiesta, senza modificare l'HttpClient condiviso.</summary>
    public sealed class FinanceApiClientFactory(HttpClient http)
    {
        private const string ApiKeyHeaderName = "X-Finance-Api-Key";

        /// <summary>Il trasporto resta gestito dal contenitore DI; la chiave non viene memorizzata nei suoi header predefiniti.</summary>
        public FinanceApiClient Create(Uri server, string? apiKey) => new(new ApiClient(http, server, apiKey is null ? null : new Dictionary<string, string> { [ApiKeyHeaderName] = apiKey }));
    }
}
