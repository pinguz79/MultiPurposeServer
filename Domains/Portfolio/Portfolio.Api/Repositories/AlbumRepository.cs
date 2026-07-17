using Microsoft.EntityFrameworkCore;
using MultiPurposeServer.Shared.Utils;
using Portfolio.Data;
using Portfolio.Data.Models;

namespace Portfolio.Api.Repositories;

public class AlbumRepository(PortfolioContext db) : IAlbumRepository
{
    public async Task<Album> CreateAlbum(string name, Guid? parent, string? path = null)
    {
        var entity = new Album { Name = name, ParentId = parent, Path = path };

        db.Albums.Add(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    public async Task<List<Album>> GetAlbums(Guid? id)
    {
        var list = await db.Albums.Where(a => a.ParentId == id).ToListAsync();
        return list;
    }

    public async Task<int> Save() => await db.SaveChangesAsync();

    public async Task<List<Album>> GetAllAlbums() => (List<Album>?)await db.Albums.ToListAsync();

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

            currentAlbum = await db.Albums.FirstOrDefaultAsync(album =>
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

    public async Task<Album?> GetById(Guid albumId) => await db.Albums.FirstOrDefaultAsync(album => album.Id == albumId);

    public async Task<Album?> UpdateName(Guid albumId, string newName)
    {
        var album = await GetById(albumId);

        if (album == null)
        {
            return null;
        }

        album.Name = newName;
        await db.SaveChangesAsync();

        return album;
    }

    public async Task<List<Album>> GetByIds(IEnumerable<Guid> ids) => await db.Albums.Where(album => ids.Contains(album.Id)).ToListAsync();
}