using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record CreateParametroContoRequest(
        [property: Normalize, Required] string Name,
        [property: EnumDefined] TipoParametroConto Type,
        [property: NormalizeChildren, Required, ValidateChildren] IReadOnlyList<ParametroContoDefinitionRequest> Definitions) : IRequest;
}
