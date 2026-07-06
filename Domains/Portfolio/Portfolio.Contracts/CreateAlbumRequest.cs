namespace Portfolio.Contracts;

public sealed record CreateAlbumRequest(string Name, Guid? Parent);
