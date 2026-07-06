using MultiPurposeServer.Models.Portfolio;

namespace MultiPurposeServer.Repositories.Portfolio
{
    public interface IAlbumRepository
    {
        Task<List<Album>> GetAllAlbums();
        Task<Album> CreateAlbum(string name, Guid? parent, string? path = null);
        Task<List<Album>> GetAlbums(Guid? id);
        Task<int> Save();
    }
}
