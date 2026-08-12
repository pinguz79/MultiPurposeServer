using Portfolio.Api.Application.Models;

namespace Portfolio.Api.Application.Services
{
    public interface ICacheService
    {
        Task<CacheClearOperationResult> Clear(bool clearAlbumRoutingCache, bool clearPhotoRoutingCache, bool clearApiResponseCache);
    }
}
