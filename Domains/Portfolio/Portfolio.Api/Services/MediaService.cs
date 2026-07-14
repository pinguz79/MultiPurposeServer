using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Portfolio.Api.Services.Models;
using Portfolio.Api.Services.Options;

namespace Portfolio.Api.Services
{
    public class MediaService(IFotoService fotoService, IOptions<PortfolioMediaOptions> options, IWebHostEnvironment environment) : IMediaService
    {
        private const uint JpegQuality = 80;

        private readonly PortfolioMediaOptions _options = options.Value;
        private readonly string _originalsRoot = ResolveRootPath(environment.ContentRootPath, options.Value.OriginalsRoot);
        private readonly string _cacheRoot = ResolveRootPath(environment.ContentRootPath, options.Value.CacheRoot);

        public Task<MediaFile?> GetImagePhoto(Guid photoId) => GetResizedPhoto(photoId, "images", _options.ImageWidth, _options.ImageHeight, false);

        public Task<MediaFile?> GetThumbnailPhoto(Guid photoId) => GetResizedPhoto(photoId, "thumbnails", _options.ThumbnailWidth, _options.ThumbnailHeight, false);

        public Task<MediaFile?> GetCoverPhoto(Guid photoId) => GetResizedPhoto(photoId, "covers", _options.CoverWidth, _options.CoverHeight, true);

        private async Task<MediaFile?> GetResizedPhoto(Guid photoId, string cacheFolder, int width, int height, bool crop)
        {
            var photo = await fotoService.GetById(photoId);

            if (photo == null || string.IsNullOrWhiteSpace(photo.RelativePath))
            {
                return null;
            }

            var sourcePath = GetSafePath(_originalsRoot, photo.RelativePath);

            if (!File.Exists(sourcePath))
            {
                return null;
            }

            var cachePath = GetCachePath(photoId, cacheFolder, width, height);

            if (!File.Exists(cachePath))
            {
                await GenerateImage(sourcePath, cachePath, width, height, crop);
            }

            return new MediaFile
            {
                FilePath = cachePath,
                ContentType = "image/jpeg"
            };
        }

        private static async Task GenerateImage(string sourcePath, string cachePath, int width, int height, bool crop)
        {
            var cacheDirectory = Path.GetDirectoryName(cachePath);

            if (string.IsNullOrWhiteSpace(cacheDirectory))
            {
                throw new InvalidOperationException($"Impossibile determinare la directory della cache dal percorso '{cachePath}'.");
            }

            Directory.CreateDirectory(cacheDirectory);

            if (Directory.Exists(cachePath))
            {
                throw new InvalidOperationException($"Il percorso destinato al file immagine è occupato da una directory: '{cachePath}'.");
            }

            using var image = new MagickImage(sourcePath);

            image.AutoOrient();

            if (crop)
            {
                ResizeAndCrop(image, width, height);
            }
            else
            {
                ResizeToFit(image, width, height);
            }

            image.Strip();
            image.Format = MagickFormat.Jpeg;
            image.Quality = JpegQuality;

            await image.WriteAsync(cachePath);
        }

        private static void ResizeToFit(MagickImage image, int width, int height)
        {
            var geometry = new MagickGeometry((uint)width, (uint)height)
            {
                IgnoreAspectRatio = false,
                Greater = true
            };

            image.Resize(geometry);
        }

        private static void ResizeAndCrop(MagickImage image, int width, int height)
        {
            var geometry = new MagickGeometry((uint)width, (uint)height)
            {
                FillArea = true
            };

            image.Resize(geometry);
            image.Extent((uint)width, (uint)height, Gravity.Center);
        }

        private string GetCachePath(Guid photoId, string cacheFolder, int width, int height)
        {
            var fileName = $"{photoId}_{width}x{height}.jpg";
            return Path.Combine(_cacheRoot, cacheFolder, fileName);
        }

        private static string GetSafePath(string root, string relativePath)
        {
            var fullRoot = Path.GetFullPath(root);
            var fullPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath));
            var rootWithSeparator = Path.EndsInDirectorySeparator(fullRoot) ? fullRoot : fullRoot + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid media path.");
            }

            return fullPath;
        }

        private static string ResolveRootPath(string contentRootPath, string configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException("Il percorso configurato non può essere vuoto.");
            }

            return Path.IsPathRooted(configuredPath) ? Path.GetFullPath(configuredPath) : Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
        }
    }
}