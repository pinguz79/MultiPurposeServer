using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkCreateParametroContoItem(
        [property: Required] int RequestId,
        [property: Normalize, Required] string Name,
        [property: Required] TipoParametroConto Type,
        [property: NormalizeChildren, Required, ValidateChildren] IReadOnlyList<ParametroContoDefinitionRequest> Definitions) : IRequest;
}
