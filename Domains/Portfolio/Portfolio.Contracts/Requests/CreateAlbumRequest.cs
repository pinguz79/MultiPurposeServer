namespace Portfolio.Contracts.Requests;

public sealed record CreateAlbumRequest(string Name, Guid? Parent, string? Description = null);
