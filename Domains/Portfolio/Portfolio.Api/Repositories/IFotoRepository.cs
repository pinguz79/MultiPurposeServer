using MultiPurposeServer.Shared.Models;
using Portfolio.Data.Models;

namespace Portfolio.Api.Repositories;

public interface IFotoRepository
{
    Task<Foto> CreatePhoto(Guid albumId, string fileName);
    Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize);
    Task<Foto?> GetById(Guid photoId);
}
