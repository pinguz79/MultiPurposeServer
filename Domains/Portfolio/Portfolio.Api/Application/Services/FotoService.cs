using MultiPurposeServer.Shared.Models;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Data.Models;

namespace Portfolio.Api.Services
{
    public class FotoService(IFotoRepository fotoRepository) : BaseService<Foto>(fotoRepository), IFotoService
    {
        public Task<List<Foto>> GetByAlbum(Guid albumId) => fotoRepository.GetByAlbum(albumId);

        public Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize) => fotoRepository.GetByAlbumId(albumId, page, pageSize);

        public async Task<List<Foto>> GetMissingDescriptions()
        {
            return (await fotoRepository.GetMissingDescriptions());
        }

        public Task<Foto> UpdateDescription(Guid photoId, string description) => fotoRepository.UpdateDescription(photoId, description);
    }
}
