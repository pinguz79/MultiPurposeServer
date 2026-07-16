using MultiPurposeServer.Shared.Models;
using Portfolio.Api.Repositories;
using Portfolio.Data.Models;

namespace Portfolio.Api.Services
{
    public class FotoService(IFotoRepository fotoRepository) : IFotoService
    {
        public Task<List<Foto>> GetByAlbum(Guid albumId) => fotoRepository.GetByAlbum(albumId);

        public Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize) => fotoRepository.GetByAlbumId(albumId, page, pageSize);

        public Task<Foto?> GetById(Guid photoId) => fotoRepository.GetById(photoId);

        public Task<Foto?> UpdateDescription(Guid photoId, string? description) => fotoRepository.UpdateDescription(photoId, description);
    }
}
