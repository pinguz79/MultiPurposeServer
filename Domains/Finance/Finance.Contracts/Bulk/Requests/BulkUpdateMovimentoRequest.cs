using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Requests;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateMovimentoRequest(BulkOptions Options, IReadOnlyCollection<BulkUpdateMovimentoItem> Items) : BulkRequest<BulkUpdateMovimentoItem>(Options, Items);
}
