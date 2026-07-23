using Microsoft.Extensions.Options;
using Portfolio.Api.Application.Operations;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Data.Models;
using System.Text.RegularExpressions;

namespace Portfolio.Api.Services
{
    public class AlbumService(IAlbumRepository albumRepository, IFotoRepository fotoRepository, IOptions<PortfolioAlbumOptions> options) : IAlbumService
    {
        private readonly string _rootPath = ResolveRootPath(options.Value.RootPath);

        public async Task<IApplicationOperation> BeginOperation()
        {
            var transaction = await albumRepository.BeginTransaction();
            return new ApplicationOperation(transaction);
        }

        public async Task<List<Album>> GetAlbums(Guid? id) => await albumRepository.GetAlbums(id);

        public async Task<Album> CreateAlbum(string name, Guid? parent)
        {
            var album = await albumRepository.CreateAlbum(name, parent, NormalizeName(name));
            var fullPath = BuildAlbumPath(album);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return album;
        }

        public async Task AmendDirectoryTree()
        {
            var allAlbums = await albumRepository.GetAll();
            var albumsByParent = allAlbums.GroupBy(album => album.ParentId).ToDictionary(group => group.Key ?? Guid.Empty, group => group.ToList());

            await SyncFolderToDb(_rootPath, null, albumsByParent);
            await albumRepository.Save();
        }

        public Task<Album?> ResolvePath(string path) => albumRepository.ResolvePath(path);

        public Task<Album?> GetById(Guid albumId) => albumRepository.GetById(albumId);

        public Task<Album> UpdateName(Guid albumId, string name) => albumRepository.UpdateName(albumId, name);

        public Task<Album> UpdateDescription(Guid albumId, string description) => albumRepository.UpdateDescription(albumId, description);

        public async Task<List<Album>> GetByNamePattern(string pattern)
        {
            Regex regex;

            try
            {
                regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException("Invalid regular expression.", nameof(pattern), exception);
            }

            return (await albumRepository.GetAll()).Where(album => regex.IsMatch(album.Name)).ToList();
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

            return Path.Combine(_rootPath, Path.Combine(stack.ToArray()));
        }

        private async Task SyncFolderToDb(string currentPath, Album? parent, Dictionary<Guid, List<Album>> albumsByParent)
        {
            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
            }

            var parentId = parent?.Id ?? Guid.Empty;
            var albums = albumsByParent.TryGetValue(parentId, out var value) ? value : [];
            var albumsByNormalizedName = albums.ToDictionary(album => album.Path ?? NormalizeName(album.Name), StringComparer.OrdinalIgnoreCase);

            var foldersByNormalizedName = Directory.GetDirectories(currentPath)
                .Select(Path.GetFileName)
                .Where(folderName => !string.IsNullOrWhiteSpace(folderName) && !folderName.StartsWith("cache", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(folderName => folderName!, StringComparer.OrdinalIgnoreCase);

            foreach (var album in albums)
            {
                album.Path ??= NormalizeName(album.Name);
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
                {
                    continue;
                }

                var album = await albumRepository.CreateAlbum(folderName, parent?.Id, folderName);

                albums.Add(album);
                albumsByNormalizedName.Add(folderName, album);

                if (!albumsByParent.ContainsKey(parentId))
                {
                    albumsByParent[parentId] = albums;
                }
            }

            if (parent != null)
            {
                var dbPhotoNames = parent.Photos.Select(photo => photo.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var file in Directory.GetFiles(currentPath, "*.jpg"))
                {
                    var fileName = Path.GetFileName(file);

                    if (dbPhotoNames.Contains(fileName))
                    {
                        continue;
                    }

                    var photo = await fotoRepository.CreatePhoto(parent.Id, fileName);
                    parent.Photos.Add(photo);
                    dbPhotoNames.Add(fileName);
                }
            }

            foreach (var album in albums.ToList())
            {
                var childPath = Path.Combine(currentPath, album.Path ?? NormalizeName(album.Name));
                await SyncFolderToDb(childPath, album, albumsByParent);
            }
        }

        private static string NormalizeName(string name) => name.Trim().Replace(' ', '-');

        private static string ResolveRootPath(string configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException("Portfolio Albums RootPath cannot be empty.");
            }

            return Path.GetFullPath(configuredPath);
        }
    }
}