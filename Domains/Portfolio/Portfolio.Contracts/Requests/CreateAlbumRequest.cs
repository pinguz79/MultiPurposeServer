namespace Portfolio.Contracts.Requests
{
    public sealed record CreateAlbumRequest(string Name, Guid? Parent = null, string? Description = null);
}
