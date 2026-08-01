using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Portfolio.Contracts.Requests;

public sealed record UpdatePhotoRequest([property: Normalize, Required] string? Description) : IRequest
{
}
