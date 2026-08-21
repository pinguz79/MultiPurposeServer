using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Models;
using MultiPurposeServer.Shared.Persistence.Transactions;
using MultiPurposeServer.Shared.Utils;

using Portfolio.DataModel;
using Portfolio.DataModel.Enums;
using Portfolio.DataModel.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public class FotoRepository(PortfolioContext db, IPersistenceCoordinator persistence) : BaseRepository<Foto>(db, persistence), IFotoRepository
    {
        #region Create e Delete

        public async Task<Foto> CreatePhoto(Guid albumId, string fileName, string? description = null) => await Add(new Foto { AlbumId = albumId, FileName = fileName, Description = description });

        public async Task Delete(Guid photoId) => await Remove(photoId);

        #endregion

        #region Get

        public async Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize) => await Query(photo => photo.AlbumId == albumId)
            .OrderBy(photo => photo.FileName)
            .ToPagedResultAsync(page, pageSize);

        public async Task<List<Foto>> GetByAlbum(Guid albumId) => await Query(photo => photo.AlbumId == albumId)
            .OrderBy(photo => photo.FileName)
            .ToListAsync();

        #endregion

        #region Update

        public async Task<Foto> UpdateDescription(Guid photoId, string? description) =>
            await Update(
                photoId,
                photo => photo.Description = NormalizeRequiredString(
                    description, nameof(description), "Photo description"));

        public async Task<Foto> UpdateContentRating(Guid photoId, PhotoContentRating contentRating) => await Update(photoId, photo => photo.ContentRating = contentRating);

        public async Task<List<Foto>> GetMissingDescriptions() => await Query(photo => string.IsNullOrEmpty(photo.Description ?? "")).ToListAsync();
        #endregion

    }
}
