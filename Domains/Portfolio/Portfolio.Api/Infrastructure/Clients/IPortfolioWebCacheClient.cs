using Portfolio.Api.Application.Models;

namespace Portfolio.Api.Infrastructure.Clients
{
    public interface IPortfolioWebCacheClient
    {
        Task<CacheClearOperationResult> Clear(bool clearAlbumRoutingCache, bool clearPhotoRoutingCache, bool clearApiResponseCache);
    }
}
