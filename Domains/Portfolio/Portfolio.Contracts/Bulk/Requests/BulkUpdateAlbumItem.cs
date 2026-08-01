using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Portfolio.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateAlbumItem([property: Required] Guid Id, [property: Normalize, RequiredAtLeastOne] string? Name, [property: Normalize, RequiredAtLeastOne] string? Description) : IRequest
    {
    }
}
