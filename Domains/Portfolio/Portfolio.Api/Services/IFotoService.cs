using MultiPurposeServer.Shared.Models;
using Portfolio.Data.Models;

namespace Portfolio.Api.Services
{
    public interface IFotoService
    {
        Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize);
        Task<Foto?> GetById(Guid photoId);
    }
}
