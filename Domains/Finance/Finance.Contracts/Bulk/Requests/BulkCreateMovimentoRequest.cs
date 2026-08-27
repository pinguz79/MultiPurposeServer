using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkCreateMovimentoRequest(
        [property: Required, ValidateChildren] BulkOptions Options,
        [property: NormalizeChildren, Required, UniqueBy("RequestId")] IReadOnlyCollection<BulkCreateMovimentoItem> Items) : IRequest, IBulk<BulkCreateMovimentoItem>;
}
