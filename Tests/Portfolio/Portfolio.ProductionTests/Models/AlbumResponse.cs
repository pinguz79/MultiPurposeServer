namespace Portfolio.ProductionTests
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
