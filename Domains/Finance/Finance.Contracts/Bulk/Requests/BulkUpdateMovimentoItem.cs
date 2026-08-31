using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateMovimentoItem(
        [property: Required] Guid Id,
        [property: RequiredAtLeastOne] DateOnly? Date,
        [property: Normalize, RequiredAtLeastOne] string? Description,
        [property: Normalize, RequiredAtLeastOne] string? Formula,
        [property: Normalize, RequiredAtLeastOne] string? CategoryName = null,
        [property: RequiredAtLeastOne] bool? ClearCategory = null) : IRequest;
}
