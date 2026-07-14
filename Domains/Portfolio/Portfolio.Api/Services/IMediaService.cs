using Portfolio.Api.Services.Models;

namespace Portfolio.Api.Services
{
    public interface IMediaService
    {
        Task<MediaFile?> GetCoverPhoto(Guid photoId);
        Task<MediaFile?> GetImagePhoto(Guid photoId);
        Task<MediaFile?> GetThumbnailPhoto(Guid photoId);
    }
}