namespace Portfolio.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateAlbumItem(Guid Id, string? Name, string? Description);
}
