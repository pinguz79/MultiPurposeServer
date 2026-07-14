using Portfolio.Data.Models;

namespace Portfolio.Api.Services;

public interface IAlbumService
{
    Task AmendDirectoryTree();
    Task<Album> CreateAlbum(string name, Guid? parent);
    Task<List<Album>> GetAlbums(Guid? id);
    Task<Album?> ResolvePath(string path);
}
