namespace Portfolio.Contracts.Models.Bulk;

public class BulkUpdateAlbumNameRequest
{
    public List<BulkUpdateAlbumNameItem> Items { get; set; } = [];
}
