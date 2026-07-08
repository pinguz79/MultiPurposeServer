using Portfolio.Data;
using Portfolio.Data.Models;

namespace Portfolio.Api.Repositories;

public class FotoRepository : IFotoRepository
{
    private readonly PortfolioContext _db;
    public FotoRepository(PortfolioContext db) => _db = db;

    public async Task<Foto> CreatePhoto(Guid albumId, string fileName)
    {
        var entity = new Foto { AlbumId = albumId, FileName = fileName };
        _db.Foto.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }
}
