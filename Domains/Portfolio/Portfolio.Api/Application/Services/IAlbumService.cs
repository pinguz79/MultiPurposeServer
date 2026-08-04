using Portfolio.Data.Models;

namespace Portfolio.Api.Application.Services
{
    public interface IAlbumService: IService<Album>
    {
        Task AmendDirectoryTree();
        Task<Album> CreateAlbum(string name, Guid? parent, string? description = null);
        Task<List<Album>> GetAlbums(Guid? id);
        Task<List<Album>> GetByNamePattern(string pattern);
        Task<Album?> ResolvePath(string path);
        Task<Album> UpdateName(Guid albumId, string name);
        Task<Album> UpdateDescription(Guid albumId, string description);
    }
}