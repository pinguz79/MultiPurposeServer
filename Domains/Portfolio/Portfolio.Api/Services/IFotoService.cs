using MultiPurposeServer.Shared.Models;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Data.Models;

namespace Portfolio.Api.Services
{
    public interface IFotoService
    {
        Task<List<Foto>?> BulkUpdateDescriptions(List<BulkUpdateAlbumNameItem> items);
        Task<List<Foto>> GetByAlbum(Guid albumId);
        Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize);
        Task<List<Foto>> GetMissingDescriptions();
        Task<Foto?> GetById(Guid photoId);
        Task<Foto> UpdateDescription(Guid photoId, string? description);
    }
}
