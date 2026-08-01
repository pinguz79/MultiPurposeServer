using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Portfolio.Contracts.Requests
{
    public sealed record CreateAlbumRequest([property: Required, Normalize] string Name, Guid? Parent = null, [property: Normalize] string? Description = null) : IRequest
    {
    }
}