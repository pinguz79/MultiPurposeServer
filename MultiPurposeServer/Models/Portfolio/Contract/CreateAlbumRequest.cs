namespace MultiPurposeServer.Models.Portfolio.Contract
{
    public sealed record CreateAlbumRequest(string Name, Guid? Parent);
}
