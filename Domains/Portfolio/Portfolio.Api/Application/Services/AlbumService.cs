using Microsoft.Extensions.Options;
using Portfolio.Api.Application.Operations;
using Portfolio.Api.Application.Diagnostics;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Data.Models;
using System.Text.RegularExpressions;

namespace Portfolio.Api.Services
{
    public class AlbumService(IAlbumRepository albumRepository, IFotoRepository fotoRepository, IOptions<PortfolioAlbumOptions> options, IAlbumSyncReportStore reportStore) : BaseService<Album>(albumRepository), IAlbumService
    {
        private readonly string _rootPath = ResolveRootPath(options.Value.RootPath);
        private readonly PortfolioAlbumOptions _options = options.Value;

        public async Task<List<Album>> GetAlbums(Guid? id) => await albumRepository.GetAlbums(id);

        public async Task<Album> CreateAlbum(string name, Guid? parent, string? description = null)
        {
            if (parent.HasValue)
            {
                Album parentAlbum = await albumRepository.GetById(parent.Value) ?? throw new KeyNotFoundException($"Album '{parent.Value}' was not found.");
                if (parentAlbum.Photos.Count > 0)
                {
                    throw new InvalidOperationException($"Album '{parentAlbum.Name}' contains photos and cannot contain child albums.");
                }
            }

            var album = await albumRepository.CreateAlbum(name, parent, NormalizeName(name), description);
            var fullPath = BuildAlbumPath(album);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return album;
        }

        public async Task<AlbumSyncReport> AmendDirectoryTree()
        {
            var report = new AlbumSyncReport { Strategy = _options.MissingPhotoStrategy };

            try
            {
                if (!Directory.Exists(_rootPath) && _options.MissingPhotoStrategy == MissingPhotoStrategy.DeleteDatabaseEntity)
                {
                    throw new InvalidOperationException($"Album root '{_rootPath}' does not exist. Destructive reconciliation has been aborted.");
                }

                var allAlbums = await albumRepository.GetAll();
                await using var transaction = await albumRepository.BeginTransaction();
                await ReconcileMissingPhotos(allAlbums, report);

                var albumsByParent = allAlbums.GroupBy(album => album.ParentId).ToDictionary(group => group.Key ?? Guid.Empty, group => group.ToList());
                await SyncFolderToDb(_rootPath, null, albumsByParent, report);
                await albumRepository.SaveIfRequired();
                await transaction.Commit();

                report.Status = report.Findings.Any(finding => finding.Severity == "Error")
                    ? AlbumSyncStatus.Degraded
                    : AlbumSyncStatus.Healthy;
                return report;
            }
            catch (Exception exception)
            {
                report.Status = AlbumSyncStatus.Unhealthy;
                report.Findings.Add(new AlbumSyncFinding("SynchronizationFailure", "Error", Guid.Empty, null, _rootPath, exception.Message, "Aborted"));
                throw;
            }
            finally
            {
                report.CompletedAt = DateTimeOffset.UtcNow;
                await reportStore.Write(report);
            }
        }

        public Task<Album?> ResolvePath(string path) => albumRepository.ResolvePath(path);

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

        private async Task ReconcileMissingPhotos(IEnumerable<Album> albums, AlbumSyncReport report)
        {
            var missingPhotos = albums
                .SelectMany(album => album.Photos.Select(photo => (Album: album, Photo: photo, ExpectedPath: Path.Combine(BuildAlbumPath(album), photo.FileName))))
                .Where(item => !File.Exists(item.ExpectedPath))
                .ToList();

            report.MissingPhotos = missingPhotos.Count;

            if (_options.MissingPhotoStrategy == MissingPhotoStrategy.DeleteDatabaseEntity && missingPhotos.Count > _options.MaxMissingPhotoDeletions)
            {
                throw new InvalidOperationException($"Found {missingPhotos.Count} missing photos, exceeding the configured deletion limit of {_options.MaxMissingPhotoDeletions}. No photo was deleted.");
            }

            foreach (var item in missingPhotos)
            {
                var delete = _options.MissingPhotoStrategy == MissingPhotoStrategy.DeleteDatabaseEntity;
                report.Findings.Add(new AlbumSyncFinding(
                    "MissingPhoto",
                    delete ? "Warning" : "Error",
                    item.Album.Id,
                    item.Photo.Id,
                    item.ExpectedPath,
                    $"Database photo '{item.Photo.FileName}' is missing from the filesystem.",
                    delete ? "DeletedDatabaseEntity" : "KeptDatabaseEntity"));

                if (!delete)
                {
                    continue;
                }

                await fotoRepository.Delete(item.Photo.Id);
                item.Album.Photos.Remove(item.Photo);
                report.PhotosDeleted++;
            }
        }

        private async Task SyncFolderToDb(string currentPath, Album? parent, Dictionary<Guid, List<Album>> albumsByParent, AlbumSyncReport report)
        {
            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
                report.FoldersCreated++;
            }

            var parentId = parent?.Id ?? Guid.Empty;
            var albums = albumsByParent.TryGetValue(parentId, out var value) ? value : [];
            var albumsByNormalizedName = albums.ToDictionary(album => album.Path ?? NormalizeName(album.Name), StringComparer.OrdinalIgnoreCase);

            var foldersByNormalizedName = Directory.GetDirectories(currentPath)
                .Select(Path.GetFileName)
                .Where(folderName => !string.IsNullOrWhiteSpace(folderName) && !folderName.StartsWith("cache", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(folderName => folderName!, StringComparer.OrdinalIgnoreCase);
            
            var photoFiles = Directory.GetFiles(currentPath, "*.jpg");
            if (parent is not null)
            {
                var containsChildAlbums = albums.Count > 0 || foldersByNormalizedName.Count > 0;
                var containsPhotos = parent.Photos.Count > 0 || photoFiles.Length > 0;

                if (containsChildAlbums && containsPhotos)
                {
                    report.Findings.Add(new AlbumSyncFinding(
                        "MixedAlbumContent",
                        "Error",
                        parent.Id,
                        null,
                        currentPath,
                        $"Album '{parent.Name}' contains both child albums and photos.",
                        "SkippedAlbumSynchronization"));
                    return;
                }
            }

            foreach (var album in albums)
            {
                album.Path ??= NormalizeName(album.Name);
                var normalizedName = album.Path;

                if (!foldersByNormalizedName.ContainsKey(normalizedName))
                {
                    Directory.CreateDirectory(Path.Combine(currentPath, normalizedName));
                    report.FoldersCreated++;
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
                report.AlbumsCreated++;

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

                foreach (var file in photoFiles)
                {
                    var fileName = Path.GetFileName(file);

                    if (dbPhotoNames.Contains(fileName))
                    {
                        continue;
                    }

                    var photo = await fotoRepository.CreatePhoto(parent.Id, fileName);
                    report.PhotosCreated++;
                    parent.Photos.Add(photo);
                    dbPhotoNames.Add(fileName);
                }
            }

            foreach (var album in albums.ToList())
            {
                var childPath = Path.Combine(currentPath, album.Path ?? NormalizeName(album.Name));
                await SyncFolderToDb(childPath, album, albumsByParent, report);
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
