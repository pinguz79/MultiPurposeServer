using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Data.Models;

namespace Portfolio.Api.Repositories;

public class AlbumRepository : IAlbumRepository
{
    private readonly PortfolioContext _db;
    public AlbumRepository(PortfolioContext db) => _db = db;

    public async Task<Album> CreateAlbum(string name, Guid? parent, string? path = null)
    {
        var entity = new Album { Name = name, ParentId = parent, Path = path };

        _db.Albums.Add(entity);
        await _db.SaveChangesAsync();

        return entity;
    }

    public async Task<List<Album>> GetAlbums(Guid? id)
    {
        var list = await _db.Albums.Where(a => a.ParentId == id).ToListAsync();
        return list;
    }

    public async Task<int> Save() => await _db.SaveChangesAsync();

    public async Task<List<Album>> GetAllAlbums()
    {
        var list = await _db.Albums.ToListAsync();
        return list;
    }
}