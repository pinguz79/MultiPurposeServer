using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Requests;

namespace Portfolio.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateFotoRequest(BulkOptions Options, IReadOnlyCollection<BulkUpdateFotoItem> Items) : BulkRequest<BulkUpdateFotoItem>(Options, Items);
}
