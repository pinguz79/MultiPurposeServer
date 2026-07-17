namespace Portfolio.Contracts.Bulk.Requests;

public sealed record BulkUpdateAlbumNameRequest(List<BulkUpdateAlbumNameItem> Items);
