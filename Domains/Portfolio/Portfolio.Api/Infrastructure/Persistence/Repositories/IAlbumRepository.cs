using Portfolio.Data.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public interface IAlbumRepository: IRepository<Album>
    {
        Task<Album> CreateAlbum(string name, Guid? parent, string? path = null, string? description = null);
        Task<List<Album>> GetAlbums(Guid? id);
        Task<Album?> ResolvePath(string path);
        Task<Album> UpdateName(Guid albumId, string name);
        Task<Album> UpdateDescription(Guid albumId, string description);
    }
}
