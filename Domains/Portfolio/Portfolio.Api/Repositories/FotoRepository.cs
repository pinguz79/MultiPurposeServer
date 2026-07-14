using Microsoft.EntityFrameworkCore;
using MultiPurposeServer.Shared.Models;
using Portfolio.Data;
using Portfolio.Data.Models;

namespace Portfolio.Api.Repositories;

public class FotoRepository(PortfolioContext db) : IFotoRepository
{
    public async Task<Foto> CreatePhoto(Guid albumId, string fileName)
    {
        var entity = new Foto { AlbumId = albumId, FileName = fileName };
        db.Foto.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<PagedResult<Foto>> GetByAlbumId(Guid albumId, int page, int pageSize)
    {
        var query = db.Foto.Where(photo => photo.AlbumId == albumId).OrderBy(photo => photo.FileName);

        var totalItems = await query.CountAsync();

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Foto>(items, totalItems);
    }

    public async Task<Foto?> GetById(Guid photoId) => await db.Foto.FindAsync(photoId);
}
