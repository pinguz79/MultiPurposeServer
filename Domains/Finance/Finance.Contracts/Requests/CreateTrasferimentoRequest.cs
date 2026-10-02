using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record CreateTrasferimentoRequest(
        [property: Normalize, Required] string OrigineName,
        [property: Normalize, Required] string DestinazioneName,
        decimal Amount, DateOnly Date,
        [property: Normalize, Required] string Description,
        bool IsConfirmed = false) : IRequest;
}
