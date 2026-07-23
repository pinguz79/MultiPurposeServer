using MultiPurposeServer.Shared.Models;
using Portfolio.Api.Infrastructure.Persistence.Transactions;
using Portfolio.Data.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories;

public interface IFotoRepository
{
    Task<IPersistenceTransaction> BeginTransaction();
    Task<Foto> CreatePhoto(Guid albumId, string fileName, string? description = null);
    Task<List<Foto>> GetAll();
    Task<List<Foto>> GetByAlbum(Guid albumId);
    Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize);
    Task<Foto?> GetById(Guid photoId);
    Task<List<Foto>> GetByIds(IEnumerable<Guid> photoIds);
    Task<List<Foto>> GetMissingDescriptions();
    Task<int> Save();
    Task<Foto?> UpdateDescription(Guid photoId, string? description);
}
