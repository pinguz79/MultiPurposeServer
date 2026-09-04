using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record ConfigureCartaASaldoRequest(
        decimal Plafond,
        decimal PercentualeScoperto,
        int ChiusuraCiclo,
        int Addebito,
        int RipristinoPlafond,
        [property: Normalize, Required] string ContoAddebitoName,
        DateOnly ValidFrom,
        DateOnly ValidTo) : IRequest;
}
