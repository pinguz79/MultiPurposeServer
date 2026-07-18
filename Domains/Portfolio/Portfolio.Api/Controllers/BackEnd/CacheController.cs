using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Services;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Controllers.BackEnd
{
    [Route("Portfolio/BackEnd/[controller]")]
    [ApiController]
    public class CacheController(ICacheService cacheService, ILogger<CacheController> logger) : PortfolioBackEndControllerBase(logger)
    {
        [HttpPost("Clear")]
        public async Task<IActionResult> ClearCache(CacheClearRequest request)
        {
            if (!request.ClearAlbumRoutingCache && !request.ClearPhotoRoutingCache && !request.ClearApiResponseCache)
            {
                return BadRequest("At least one cache must be selected.");
            }

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
