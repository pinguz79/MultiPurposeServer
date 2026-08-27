using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record CreateVoceRicorrenteRequest(
        [property: Normalize, Required] string Name,
        [property: NormalizeChildren, Required, ValidateChildren] IReadOnlyList<VoceRicorrenteDefinitionRequest> Definitions) : IRequest;
}
