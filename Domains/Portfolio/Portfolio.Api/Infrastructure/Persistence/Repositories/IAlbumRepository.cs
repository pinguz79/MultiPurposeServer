using Portfolio.Data.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public interface IAlbumRepository: IRepository<Album>
    {
        Task<Album> CreateAlbum(string name, Guid? parent, string? path = null, string? description = null);
        Task DeleteAlbum(Guid albumId);
        Task<List<Album>> GetAlbums(Guid? id);
        Task<List<Album>> GetMissingDescriptions();
        Task<Album?> ResolvePath(string path);
        Task<Album> UpdateName(Guid albumId, string name);
        Task<Album> UpdateDescription(Guid albumId, string description);
    }
}
