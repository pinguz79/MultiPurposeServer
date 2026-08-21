using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.Transactions;
using MultiPurposeServer.Shared.Utils;

using Portfolio.DataModel;
using Portfolio.DataModel.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public class AlbumRepository(PortfolioContext db, IPersistenceCoordinator persistence) : BaseRepository<Album>(db, persistence), IAlbumRepository
    {
        #region Create e Delete

        public async Task<Album> CreateAlbum(
            string name, Guid? parent, string? path = null, string? description = null) =>
            await Add(new Album { Name = name, ParentId = parent, Path = path, Description = description });

        public async Task DeleteAlbum(Guid albumId) => await Remove(albumId);

        #endregion

        #region Get

        public async Task<List<Album>> GetAlbums(Guid? id)
        {
            var list = await Query(a => a.ParentId == id).ToListAsync();
            return list;
        }

        public async Task<List<Album>> GetMissingDescriptions() => await Query(album => string.IsNullOrEmpty(album.Description ?? "")).ToListAsync();

        public async Task<Album?> ResolvePath(string path)
        {
            var normalizedPath = path.NormalizedPath();

            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return null;
            }

            var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            Guid? parentId = null;
            Album? currentAlbum = null;

            foreach (var segment in segments)
            {
                var normalizedSegment = segment.NormalizedPathForComparison();

                currentAlbum = await Query(album =>
                    album.ParentId == parentId &&
                    album.Path != null &&
                    album.Path.ToUpper() == normalizedSegment).FirstOrDefaultAsync();

                if (currentAlbum == null)
                {
                    return null;
                }

                parentId = currentAlbum.Id;
            }

            return currentAlbum;
        }

        #endregion

        #region Update

        public async Task<Album> UpdateName(Guid albumId, string name) => await Update(albumId, album => album.Name = NormalizeRequiredString(name, nameof(name), "Album name"));

        public async Task<Album> UpdateDescription(Guid albumId, string description) =>
            await Update(
                albumId,
                album => album.Description = NormalizeRequiredString(
                    description, nameof(description), "Album description"));
        #endregion

    }
}
