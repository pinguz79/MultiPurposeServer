using Portfolio.Api.Services.Models;

namespace Portfolio.Api.Services;

public interface ICacheService
{
    Task<CacheClearOperationResult> Clear(bool clearAlbumRoutingCache, bool clearPhotoRoutingCache, bool clearApiResponseCache);
}