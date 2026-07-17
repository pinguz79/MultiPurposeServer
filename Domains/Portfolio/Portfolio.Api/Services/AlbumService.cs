using Portfolio.Api.Repositories;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Data.Models;
using System.Text.RegularExpressions;

namespace Portfolio.Api.Services;

public class AlbumService(IAlbumRepository albumRepository, IFotoRepository fotoRepository) : IAlbumService
{
    public async Task<List<Album>> GetAlbums(Guid? id) => await albumRepository.GetAlbums(id);

    public async Task<Album> CreateAlbum(string name, Guid? parent)
    {
        var album = await albumRepository.CreateAlbum(name, parent, NormalizeName(name));

        var fullPath = BuildAlbumPath(album);

        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        return album;
    }

    private string BuildAlbumPath(Album album)
    {
        var stack = new Stack<string>();
        var current = album;

        while (current != null)
        {
            stack.Push(current.Path ?? NormalizeName(current.Name));
            current = current.Parent;
        }

        return Path.Combine("Portfolio", Path.Combine(stack.ToArray()));
    }

    public async Task AmendDirectoryTree()
    {
        var allAlbums = await albumRepository.GetAllAlbums();
        var albumsByParent = allAlbums.GroupBy(a => a.ParentId).ToDictionary(g => g.Key ?? Guid.Empty, g => g.ToList());

        await SyncFolderToDb("Portfolio", null, albumsByParent);
        await albumRepository.Save();
    }

    private async Task SyncFolderToDb(string currentPath, Album? parent, Dictionary<Guid, List<Album>> albumsByParent)
    {
        if (!Directory.Exists(currentPath))
            Directory.CreateDirectory(currentPath);

        var albums = albumsByParent.TryGetValue(parent?.Id ?? Guid.Empty, out var value) ? value : new List<Album>();

        var albumsByNormalizedName = albums.ToDictionary(a => a.Path ?? NormalizeName(a.Name), StringComparer.OrdinalIgnoreCase);

        var foldersByNormalizedName = Directory.GetDirectories(currentPath).Select(d => Path.GetFileName(d)!).Where(d => !d.StartsWith("cache", StringComparison.InvariantCultureIgnoreCase)).ToDictionary(d => d, StringComparer.OrdinalIgnoreCase);

        foreach (var album in albums)
        {
            album.Path = album.Path ?? NormalizeName(album.Name);
            var normalizedName = album.Path;

            if (!foldersByNormalizedName.ContainsKey(normalizedName))
            {
                Directory.CreateDirectory(Path.Combine(currentPath, normalizedName));
                foldersByNormalizedName.Add(normalizedName, normalizedName);
            }
        }

        foreach (var folderName in foldersByNormalizedName.Keys)
        {
            if (albumsByNormalizedName.ContainsKey(folderName))
                continue;

            var album = await albumRepository.CreateAlbum(folderName, parent?.Id, folderName);

            albums.Add(album);
            albumsByNormalizedName.Add(folderName, album);

            if (!albumsByParent.TryGetValue(parent?.Id ?? Guid.Empty, out _))
                albumsByParent[parent?.Id ?? Guid.Empty] = albums;
        }

        if (parent != null)
        {
            var dbPhotos = parent.Photos ?? new List<Foto>();
            var dbPhotoNames = dbPhotos.Select(p => p.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var fsPhotos = Directory.GetFiles(currentPath, "*.jpg");
            foreach (var file in fsPhotos)
            {
                var fileName = Path.GetFileName(file);

                if (dbPhotoNames.Contains(fileName))
                    continue;

                var photo = await fotoRepository.CreatePhoto(parent.Id, fileName);
                parent.Photos.Add(photo);
            }
        }

        foreach (var album in albums.ToList())
        {
            var childPath = Path.Combine(currentPath, album.Path ?? NormalizeName(album.Name));

            await SyncFolderToDb(childPath, album, albumsByParent);
        }
    }

    private static string NormalizeName(string name) => name.Trim().Replace(' ', '-');

    public Task<Album?> ResolvePath(string path) => albumRepository.ResolvePath(path);

    public Task<Album?> GetById(Guid albumId) => albumRepository.GetById(albumId);

    public Task<Album?> UpdateName(Guid albumId, string newName) => albumRepository.UpdateName(albumId, newName);

    public async Task<List<Album>> GetByNamePattern(string pattern)
    {
        Regex regex;

        try
        {
            regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("Invalid regular expression.", nameof(pattern), ex);
        }

        return (await albumRepository.GetAllAlbums())
            .Where(album => regex.IsMatch(album.Name))
            .ToList();
    }

    public async Task<List<Album>?> BulkUpdateNames(List<BulkUpdateAlbumNameItem> items)
    {
        var updates = items.ToDictionary(item => item.Id, item => item.NewName.Trim());

        var albums = await albumRepository.GetByIds(updates.Keys);

        if (albums.Count != updates.Count)
        {
            return null;
        }

        foreach (var album in albums)
        {
            album.Name = updates[album.Id];
        }

        await albumRepository.Save();

        return albums;
    }
}
