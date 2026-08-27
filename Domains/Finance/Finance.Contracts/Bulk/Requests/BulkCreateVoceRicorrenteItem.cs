using Finance.Contracts.Requests;

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Bulk.Requests
{
    public sealed record BulkCreateVoceRicorrenteItem(
        [property: Required] int RequestId,
        [property: Normalize, Required] string Name,
        [property: NormalizeChildren, Required, ValidateChildren] IReadOnlyList<VoceRicorrenteDefinitionRequest> Definitions) : IRequest;
}
