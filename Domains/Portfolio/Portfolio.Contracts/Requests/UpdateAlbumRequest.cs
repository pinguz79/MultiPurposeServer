using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Portfolio.Contracts.Requests;

public sealed record UpdateAlbumRequest([property: Normalize, RequiredAtLeastOne] string? Name, [property: Normalize, RequiredAtLeastOne] string? Description) : IRequest
{
}
