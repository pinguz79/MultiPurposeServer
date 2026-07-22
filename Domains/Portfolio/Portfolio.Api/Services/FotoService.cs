using MultiPurposeServer.Shared.Models;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Api.Services.Models;
using Portfolio.Data.Models;

namespace Portfolio.Api.Services
{
    public class FotoService(IFotoRepository fotoRepository) : IFotoService
    {
        public async Task<List<Foto>?> BulkUpdateDescriptions(IReadOnlyCollection<BulkUpdateItem<string>> items)
        {
            if (items.Count == 0)
            {
                return [];
            }

            var updates = items.ToDictionary(item => item.Id, item => item.Value.Trim());
            var photos = await fotoRepository.GetByIds(updates.Keys);

            if (photos.Count != updates.Count)
            {
                return null;
            }

            foreach (var photo in photos)
            {
                photo.Description = updates[photo.Id];
            }

            await fotoRepository.Save();
            return photos;
        }

        public Task<List<Foto>> GetByAlbum(Guid albumId) => fotoRepository.GetByAlbum(albumId);

        public Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize) => fotoRepository.GetByAlbumId(albumId, page, pageSize);

        public async Task<List<Foto>> GetMissingDescriptions()
        {
            return (await fotoRepository.GetMissingDescriptions());
        }

        public Task<Foto?> GetById(Guid photoId) => fotoRepository.GetById(photoId);

        public Task<Foto?> UpdateDescription(Guid photoId, string? description) => fotoRepository.UpdateDescription(photoId, description);
    }
}
