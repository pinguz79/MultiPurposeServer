using Microsoft.EntityFrameworkCore;
using MultiPurposeServer.Shared.Utils;
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

            currentAlbum = await _db.Albums.FirstOrDefaultAsync(album =>
                album.ParentId == parentId &&
                album.Path != null &&
                album.Path.ToUpper() == normalizedSegment);

            if (currentAlbum == null)
            {
                return null;
            }

            parentId = currentAlbum.Id;
        }

        return currentAlbum;
    }
}