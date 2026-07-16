namespace Portfolio.Contracts.Models.Bulk;

public class BulkUpdateAlbumNameItem
{
    public Guid Id { get; set; }
    public string NewName { get; set; } = string.Empty;
}