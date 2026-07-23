using Portfolio.Api.Application.Operations;
using Portfolio.Data.Models;

namespace Portfolio.Api.Application.Services
{
    public interface IAlbumService
    {
        Task<IApplicationOperation> BeginOperation();
        Task AmendDirectoryTree();
        Task<Album> CreateAlbum(string name, Guid? parent);
        Task<List<Album>> GetAlbums(Guid? id);
        Task<Album?> GetById(Guid albumId);
        Task<List<Album>> GetByNamePattern(string pattern);
        Task<Album?> ResolvePath(string path);
        Task<Album> UpdateName(Guid albumId, string name);
        Task<Album> UpdateDescription(Guid albumId, string description);
    }
}