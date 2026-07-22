using MultiPurposeServer.Shared.Models;
using Portfolio.Data.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories;

public interface IFotoRepository
{
    Task<Foto> CreatePhoto(Guid albumId, string fileName);
    Task<List<Foto>> GetAllPhotos();
    Task<List<Foto>> GetByAlbum(Guid albumId);
    Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize);
    Task<Foto?> GetById(Guid photoId);
    Task<List<Foto>> GetByIds(IEnumerable<Guid> photoIds);
    Task<List<Foto>> GetMissingDescriptions();
    Task<int> Save();
    Task<Foto?> UpdateDescription(Guid photoId, string? description);
}
