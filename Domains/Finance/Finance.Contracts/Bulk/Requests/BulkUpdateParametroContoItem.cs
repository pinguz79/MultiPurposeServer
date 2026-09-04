using Finance.Contracts.Requests;

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateParametroContoItem(
        [property: Normalize, Required] string Name,
        [property: NormalizeChildren, Required, ValidateChildren] IReadOnlyList<ParametroContoDefinitionRequest> Definitions) : IRequest;
}
