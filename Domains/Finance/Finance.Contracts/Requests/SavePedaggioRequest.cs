using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record SavePedaggioRequest(
        [property: Normalize, Required] string ContoName,
        DateOnly Date,
        Guid EntrataId,
        Guid UscitaId,
        [property: Normalize] string? Description = null,
        [property: Normalize] string? CategoryName = null,
        bool IsConfirmed = false) : IRequest;
}
