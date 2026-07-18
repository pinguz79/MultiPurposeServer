using MultiPurposeServer.Shared.Models;
using Portfolio.Api.Services.Models;
using Portfolio.Data.Models;

namespace Portfolio.Api.Services;

public interface IFotoService
{
    Task<List<Foto>?> BulkUpdateDescriptions(IReadOnlyCollection<BulkUpdateItem<string>> items);
    Task<List<Foto>> GetByAlbum(Guid albumId);
    Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize);
    Task<List<Foto>> GetMissingDescriptions();
    Task<Foto?> GetById(Guid photoId);
    Task<Foto?> UpdateDescription(Guid photoId, string? description);
}