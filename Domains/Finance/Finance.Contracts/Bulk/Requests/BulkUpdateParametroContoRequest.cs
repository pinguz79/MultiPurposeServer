using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateParametroContoRequest(
        [property: Required, ValidateChildren] BulkOptions Options,
        [property: NormalizeChildren, Required, UniqueBy("Name"), ValidateChildren] IReadOnlyCollection<BulkUpdateParametroContoItem> Items)
        : IRequest, IBulk<BulkUpdateParametroContoItem>;
}
