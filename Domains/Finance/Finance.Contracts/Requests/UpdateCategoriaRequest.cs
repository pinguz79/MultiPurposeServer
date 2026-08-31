using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record UpdateCategoriaRequest([property: Normalize, Required] string DisplayName) : IRequest;
}
