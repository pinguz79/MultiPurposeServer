using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkCreateMovimentoItem(
        [property: Required] int RequestId,
        [property: Required] DateOnly Date,
        [property: Normalize, Required] string Description,
        [property: Normalize, Required] string Formula) : IRequest;
}
