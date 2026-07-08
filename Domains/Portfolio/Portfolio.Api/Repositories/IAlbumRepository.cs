using Portfolio.Data.Models;

namespace Portfolio.Api.Repositories;

public interface IAlbumRepository
{
    Task<List<Album>> GetAllAlbums();
    Task<Album> CreateAlbum(string name, Guid? parent, string? path = null);
    Task<List<Album>> GetAlbums(Guid? id);
    Task<int> Save();
}
