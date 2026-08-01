using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Requests;

namespace Portfolio.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateAlbumRequest(BulkOptions Options, IReadOnlyCollection<BulkUpdateAlbumItem> Items) : BulkRequest<BulkUpdateAlbumItem>(Options, Items);
}
