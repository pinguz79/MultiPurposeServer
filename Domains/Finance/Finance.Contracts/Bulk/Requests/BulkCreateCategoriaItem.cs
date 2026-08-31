using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkCreateCategoriaItem(
        [property: Required] int RequestId,
        [property: Normalize, Required] string Name,
        [property: Normalize, Required] string DisplayName) : IRequest;
}
