using MultiPurposeServer.Shared.Models;

using Portfolio.Data.Enums;
using Portfolio.Data.Models;

namespace Portfolio.Api.Application.Services
{
    public interface IFotoService : IService<Foto>
    {
        Task<List<Foto>> GetByAlbum(Guid albumId);
        Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize);
        Task<List<Foto>> GetMissingDescriptions();
        Task<Foto> UpdateDescription(Guid photoId, string description);
        Task<Foto> UpdateContentRating(Guid photoId, PhotoContentRating contentRating);
    }
}
