using Microsoft.EntityFrameworkCore;
using MultiPurposeServer.Shared.Utils;
using Portfolio.Api.Infrastructure.Persistence.Transactions;
using Portfolio.Data;
using Portfolio.Data.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories;

public class AlbumRepository(PortfolioContext db) : IAlbumRepository, ITransactionalRepository
{
    private IDbContextTransaction? _transaction;

    public async Task<IPersistenceTransaction> BeginTransaction()
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("A repository transaction is already active.");
        }

        _transaction = await db.Database.BeginTransactionAsync();

        return new PersistenceTransaction(this);
    }

    private async Task SaveIfRequired()
    {
        if (_transaction is null)
        {
            await db.SaveChangesAsync();
        }
    }

    public async Task<Album> CreateAlbum(string name, Guid? parent, string? path = null)
    {
        var entity = new Album { Name = name, ParentId = parent, Path = path };

        db.Albums.Add(entity);
        await SaveIfRequired();

        return entity;
    }

    public async Task<List<Album>> GetAlbums(Guid? id)
    {
        var list = await db.Albums.Where(a => a.ParentId == id).ToListAsync();
        return list;
    }

    public async Task<int> Save()
    {
        if (_transaction is not null)
        {
            return 0;
        }

        return await db.SaveChangesAsync();
    }

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

    public async Task<Album> UpdateName(Guid albumId, string name)
    {
        var normalizedName = NormalizeRequiredString(name, nameof(name), "Album name");
        return await UpdateAlbum(albumId, album => album.Name = normalizedName);
    }

    public async Task<Album> UpdateDescription(Guid albumId, string description)
    {
        var normalizedDescription = NormalizeRequiredString(description, nameof(description), "Album description");
        return await UpdateAlbum(albumId, album => album.Description = normalizedDescription);
    }

    public async Task<List<Album>> GetByIds(IEnumerable<Guid> ids) => await db.Albums.Where(album => ids.Contains(album.Id)).ToListAsync();

    public async Task CommitTransaction()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No repository transaction is active.");
        }

        try
        {
            await db.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransaction()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No repository transaction is active.");
        }

        try
        {
            await _transaction.RollbackAsync();
            db.ChangeTracker.Clear();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    private async Task<Album> UpdateAlbum(Guid albumId, Action<Album> update)
    {
        var album = await GetById(albumId)
            ?? throw new KeyNotFoundException($"Album '{albumId}' was not found.");

        update(album);
        await SaveIfRequired();

        return album;
    }

    private static string NormalizeRequiredString(string value, string parameterName, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length == 0)
        {
            throw new ArgumentException($"{fieldName} cannot be empty.", parameterName);
        }

        return normalizedValue;
    }
}