using MultiPurposeServer.Client;

namespace Finance.Client
{
    /// <summary>Operazioni di lettura Finance, indipendenti dalla UI e dalla persistenza server.</summary>
    public sealed class FinanceApiClient(ApiClient api)
    {
        /// <summary>Restituisce i conti con gli indicatori calcolati dal server, senza retry né scritture.</summary>
        public async Task<IReadOnlyList<Conto>> GetConti(CancellationToken cancellationToken = default) => await api.Get("Finance/FrontEnd/Conto/List", FinanceJsonContext.Default.ListConto, cancellationToken);
    }
}
