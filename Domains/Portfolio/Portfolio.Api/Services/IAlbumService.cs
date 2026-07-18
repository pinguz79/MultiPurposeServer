using Portfolio.Api.Services.Models;
using Portfolio.Data.Models;

namespace Portfolio.Api.Services;

public interface IAlbumService
{
    Task AmendDirectoryTree();
    Task<List<Album>?> BulkUpdateNames(IReadOnlyCollection<BulkUpdateItem<string>> items);
    Task<Album> CreateAlbum(string name, Guid? parent);
    Task<List<Album>> GetAlbums(Guid? id);
    Task<Album?> GetById(Guid albumId);
    Task<List<Album>> GetByNamePattern(string pattern);
    Task<Album?> ResolvePath(string path);
    Task<Album?> UpdateName(Guid albumId, string newName);
}