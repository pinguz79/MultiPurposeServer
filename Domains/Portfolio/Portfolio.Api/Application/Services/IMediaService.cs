using Portfolio.Api.Application.Models;

namespace Portfolio.Api.Application.Services
{
    public interface IMediaService
    {
        Task<MediaFile?> GetCoverPhoto(Guid photoId);
        Task<MediaFile?> GetEditorialCoverPhoto(Guid photoId);
        Task<MediaFile?> GetImagePhoto(Guid photoId);
        Task<MediaFile?> GetThumbnailPhoto(Guid photoId);
    }
}
