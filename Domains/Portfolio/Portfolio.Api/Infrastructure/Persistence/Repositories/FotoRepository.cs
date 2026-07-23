using Microsoft.EntityFrameworkCore;
using MultiPurposeServer.Shared.Models;
using MultiPurposeServer.Shared.Utils;
using Portfolio.Data;
using Portfolio.Data.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories;

public class FotoRepository(PortfolioContext db) : BaseRepository<Foto>(db), IFotoRepository
{
    protected override DbSet<Foto> Set => db.Foto;

    public async Task<Foto> CreatePhoto(Guid albumId, string fileName, string? description = null) => await Add(new Foto { AlbumId = albumId, FileName = fileName, Description = description });

    public async Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize) => await Query(photo => photo.AlbumId == albumId)
        .OrderBy(photo => photo.FileName)
        .ToPagedResultAsync(page, pageSize);

    public async Task<List<Foto>> GetByAlbum(Guid albumId) => await Query(photo => photo.AlbumId == albumId)
        .OrderBy(photo => photo.FileName)
        .ToListAsync();

    public async Task<Foto?> UpdateDescription(Guid photoId, string? description) => await Update(photoId, photo => photo.Description = NormalizeRequiredString(description, nameof(description), "Photo description"), nameof(Foto));

    public async Task<List<Foto>> GetMissingDescriptions() => await Query(photo => string.IsNullOrEmpty(photo.Description ?? "")).ToListAsync();
}
