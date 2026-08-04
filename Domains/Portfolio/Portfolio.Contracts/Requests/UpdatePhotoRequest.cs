using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Portfolio.Contracts.Requests
{
    public sealed record UpdatePhotoRequest([property: Normalize, RequiredAtLeastOne] string? Description) : IRequest
    {
    }
}
