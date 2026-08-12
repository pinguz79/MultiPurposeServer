using Portfolio.Api.Application.Models;
using Portfolio.Api.Infrastructure.Clients;

namespace Portfolio.Api.Application.Services
{
    public sealed class CacheService(IPortfolioWebCacheClient portfolioWebCacheClient) : ICacheService
    {
        public Task<CacheClearOperationResult> Clear(
            bool clearAlbumRoutingCache, bool clearPhotoRoutingCache, bool clearApiResponseCache) =>
            portfolioWebCacheClient.Clear(clearAlbumRoutingCache, clearPhotoRoutingCache, clearApiResponseCache);
    }
}
