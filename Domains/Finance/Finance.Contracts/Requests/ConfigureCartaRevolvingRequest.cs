using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record ConfigureCartaRevolvingRequest(
        decimal Plafond,
        decimal PercentualeScoperto,
        decimal QuotaRata,
        decimal RataMinima,
        decimal Tan,
        decimal Bollo,
        decimal SogliaBollo,
        int ChiusuraCiclo,
        int Addebito,
        [property: Normalize, Required] string ContoAddebitoName,
        DateOnly ValidFrom,
        DateOnly ValidTo,
        decimal? Rata = null) : IRequest;
}
