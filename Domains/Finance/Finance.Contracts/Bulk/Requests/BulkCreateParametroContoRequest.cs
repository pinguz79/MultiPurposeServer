using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkCreateParametroContoRequest(
        [property: Required, ValidateChildren] BulkOptions Options,
        [property: NormalizeChildren, Required, UniqueBy("RequestId"), ValidateChildren] IReadOnlyCollection<BulkCreateParametroContoItem> Items)
        : IRequest, IBulk<BulkCreateParametroContoItem>;
}
