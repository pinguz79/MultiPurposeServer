using MultiPurposeServer.Models.Portfolio;

namespace MultiPurposeServer.Microservices.Portfolio
{
    public interface IAlbumService
    {
        Task AmendDirectoryTree();
        Task<Album> CreateAlbum(string name, Guid? parent);
        Task<List<Album>> GetAlbums(Guid? id);
    }
}
