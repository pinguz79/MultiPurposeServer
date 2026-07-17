using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Services
{
    public interface ICacheService
    {
        Task<ClearCacheResults> Clear(bool clearAlbumRoutingCache, bool clearPhotoRoutingCache, bool clearApiResponseCache);
    }
}
