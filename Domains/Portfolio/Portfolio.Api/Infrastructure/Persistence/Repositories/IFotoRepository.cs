using MultiPurposeServer.Shared.Models;
using Portfolio.Data.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public interface IFotoRepository : IRepository<Foto>
    {
        Task<Foto> CreatePhoto(Guid albumId, string fileName, string? description = null);
        Task<List<Foto>> GetByAlbum(Guid albumId);
        Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize);
        Task<List<Foto>> GetMissingDescriptions();
        Task Delete(Guid photoId);
        Task<Foto> UpdateDescription(Guid photoId, string? description);
    }
}
