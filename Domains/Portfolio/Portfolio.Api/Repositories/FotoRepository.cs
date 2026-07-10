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

    public async Task<Foto?> GetById(Guid photoId) => await db.Foto.FindAsync(photoId);
}
