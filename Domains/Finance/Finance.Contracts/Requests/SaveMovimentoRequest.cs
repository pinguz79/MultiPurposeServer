using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record SaveMovimentoRequest(
        [property: Normalize, Required] string ContoName,
        DateOnly Date,
        [property: Normalize, Required] string Description,
        [property: Required] string Formula,
        bool IsConfirmed,
        [property: Normalize] string? CategoryName = null) : IRequest;
}
