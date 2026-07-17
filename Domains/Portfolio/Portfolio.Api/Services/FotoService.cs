using MultiPurposeServer.Shared.Models;
using Portfolio.Api.Repositories;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Data.Models;
using System.Text.RegularExpressions;

namespace Portfolio.Api.Services
{
    public class FotoService(IFotoRepository fotoRepository) : IFotoService
    {
        public async Task<List<Foto>?> BulkUpdateDescriptions(List<BulkUpdateAlbumNameItem> items)
        {
            var updates = items.ToDictionary(item => item.Id, item => item.NewName.Trim());
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
