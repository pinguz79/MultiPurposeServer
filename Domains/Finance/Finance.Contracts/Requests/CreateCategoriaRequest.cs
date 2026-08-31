using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record CreateCategoriaRequest(
        [property: Normalize, Required] string Name,
        [property: Normalize, Required] string DisplayName) : IRequest;
}
