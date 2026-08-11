using Microsoft.Extensions.Options;
using MultiPurposeServer.Shared.Utils;
using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Options;

namespace Portfolio.Api.Application.Services
{
    public class MediaService(IFotoService fotoService, IImageResizer imageResizer, IOptions<PortfolioMediaOptions> options) : IMediaService
    {
        private readonly string _originalsRoot = PathSecurity.ResolveRootPath(options.Value.RootPath, options.Value.OriginalsRoot);
        private readonly string _cacheRoot = PathSecurity.ResolveRootPath(options.Value.RootPath, options.Value.CacheRoot);
        private readonly MediaProfile _imageProfile = new("images", options.Value.ImageWidth, options.Value.ImageHeight, false);
        private readonly MediaProfile _thumbnailProfile = new("thumbnails", options.Value.ThumbnailWidth, options.Value.ThumbnailHeight, false);
        private readonly MediaProfile _coverProfile = new("covers-top-v1", options.Value.CoverWidth, options.Value.CoverHeight, true);

        public Task<MediaFile?> GetImagePhoto(Guid photoId) => GetResizedPhoto(photoId, _imageProfile);

        public Task<MediaFile?> GetThumbnailPhoto(Guid photoId) => GetResizedPhoto(photoId, _thumbnailProfile);

        public Task<MediaFile?> GetCoverPhoto(Guid photoId) => GetResizedPhoto(photoId, _coverProfile);
        private async Task<MediaFile?> GetResizedPhoto(Guid photoId, MediaProfile profile)
        {
            var photo = await fotoService.GetById(photoId);

            if (photo == null || string.IsNullOrWhiteSpace(photo.RelativePath))
            {
                return null;
            }

            var sourcePath = PathSecurity.GetSafePath(_originalsRoot, photo.RelativePath);

            if (!File.Exists(sourcePath))
            {
                return null;
            }

            var cachePath = GetCachePath(photoId, profile.CacheFolder, profile.Width, profile.Height);

            if (!File.Exists(cachePath))
            {
                await imageResizer.Resize(sourcePath, cachePath, profile.Width, profile.Height, profile.Crop);
            }

            return new MediaFile
            {
                FilePath = cachePath,
                ContentType = "image/jpeg"
            };
        }

        private string GetCachePath(Guid photoId, string cacheFolder, int width, int height)
        {
            var fileName = $"{photoId}_{width}x{height}.jpg";
            return Path.Combine(_cacheRoot, cacheFolder, fileName);
        }
    }
}
