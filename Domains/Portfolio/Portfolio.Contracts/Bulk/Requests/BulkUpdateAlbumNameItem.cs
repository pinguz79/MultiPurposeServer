namespace Portfolio.Contracts.Bulk.Requests;

public sealed record BulkUpdateAlbumNameItem(Guid Id, string NewName);