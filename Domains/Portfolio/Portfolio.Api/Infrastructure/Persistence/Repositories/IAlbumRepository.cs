using Portfolio.Api.Infrastructure.Persistence.Transactions;
using Portfolio.Data.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories;

public interface IAlbumRepository
{
    Task<IPersistenceTransaction> BeginTransaction();
    Task<List<Album>> GetAll();
    Task<Album> CreateAlbum(string name, Guid? parent, string? path = null);
    Task<List<Album>> GetAlbums(Guid? id);
    Task<int> Save();
    Task<Album?> ResolvePath(string path);
    Task<Album?> GetById(Guid albumId);
    Task<Album> UpdateName(Guid albumId, string name);
    Task<Album> UpdateDescription(Guid albumId, string description);
    Task<List<Album>> GetByIds(IEnumerable<Guid> ids);
}
