namespace Portfolio.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateAlbumRequest(BulkUpdateAlbumOptions Options, IReadOnlyCollection<BulkUpdateAlbumItem> Items);
}
