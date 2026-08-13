namespace Portfolio.ProductionTests.Models
{
    internal sealed record AlbumResponse(
        Guid Id,
        string Name,
        string? Path,
        string? FullPath,
        Guid? ParentId,
        string Kind,
        int Children,
        int Photos);
}
