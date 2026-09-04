using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record UpdateParametroContoRequest(
        [property: NormalizeChildren, Required, ValidateChildren] IReadOnlyList<ParametroContoDefinitionRequest> Definitions) : IRequest;
}
