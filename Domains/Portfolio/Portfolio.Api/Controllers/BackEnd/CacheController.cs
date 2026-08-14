using Microsoft.AspNetCore.Mvc;

using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Controllers.BackEnd
{
    [Route("Portfolio/BackEnd/[controller]")]
    [ApiController]
    public class CacheController(ICacheService cacheService) : PortfolioBackEndControllerBase
    {
        [HttpPost("Clear")]
        public async Task<IActionResult> ClearCache(CacheClearRequest request)
        {
            var result = await cacheService.Clear(request.ClearAlbumRoutingCache, request.ClearPhotoRoutingCache, request.ClearApiResponseCache);

            return Ok(new CacheClearResult()
            {
                AlbumRoutingEntriesDeleted = result.AlbumRoutingEntriesDeleted,
                PhotoRoutingEntriesDeleted = result.PhotoRoutingEntriesDeleted,
                ApiResponseEntriesDeleted = result.ApiResponseEntriesDeleted
            });
        }
    }
}
