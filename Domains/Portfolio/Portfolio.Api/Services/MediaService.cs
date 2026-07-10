using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Portfolio.Api.Services.Models;
using Portfolio.Api.Services.Options;

namespace Portfolio.Api.Services
{
    public class MediaService(IFotoService fotoService, IOptions<PortfolioMediaOptions> options, IWebHostEnvironment environment) : IMediaService
    {
        private readonly PortfolioMediaOptions _options = options.Value;
        private readonly string _originalsRoot = ResolveRootPath(environment.ContentRootPath, options.Value.OriginalsRoot);
        private readonly string _cacheRoot = ResolveRootPath(environment.ContentRootPath, options.Value.CacheRoot);

        public async Task<MediaFile?> GetCoverPhoto(Guid photoId)
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

            var cachePath = GetCoverCachePath(photoId);

            if (!File.Exists(cachePath))
            {
                if (string.IsNullOrWhiteSpace(cachePath))
                {
                    throw new InvalidOperationException("Impossibile determinare la directory della cache.");
                }

                var cacheDirectory = Path.GetDirectoryName(cachePath);
                Directory.CreateDirectory(cacheDirectory);

                using var image = new MagickImage(sourcePath);

                image.AutoOrient();

                var geometry = new MagickGeometry((uint)_options.CoverWidth, (uint)_options.CoverHeight)
                {
                    FillArea = true
                };

                image.Resize(geometry);

                image.Extent((uint)_options.CoverWidth, (uint)_options.CoverHeight, Gravity.Center);

                image.Strip();
                image.Format = MagickFormat.Jpeg;
                image.Quality = 80;

                await image.WriteAsync(cachePath);
            }

            return new MediaFile
            {
                FilePath = cachePath,
                ContentType = "image/jpeg"
            };
        }

        private string GetCoverCachePath(Guid photoId) => Path.Combine(_cacheRoot, "covers", $"{photoId}_{_options.CoverWidth}x{_options.CoverHeight}.jpg");

        private static string GetSafePath(string root, string relativePath)
        {
            var fullRoot = Path.GetFullPath(root);
            var fullPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath));

            if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
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