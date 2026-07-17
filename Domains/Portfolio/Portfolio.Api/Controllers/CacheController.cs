using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Services;
using Portfolio.Contracts.Requests;

namespace Portfolio.Api.Controllers
{
    [Route("Portfolio/BackEnd/[controller]")]
    [ApiController]
    public class CacheController(ICacheService cacheService, ILogger<CacheController> logger) : PortfolioControllerBase(logger)
    {
        [HttpPost("Clear")]
        public async Task<IActionResult> ClearCache(ClearCacheRequest request)
        {
            if (!request.ClearAlbumRoutingCache && !request.ClearPhotoRoutingCache && !request.ClearApiResponseCache)
            {
                return BadRequest("At least one cache must be selected.");
            }

            var result = await cacheService.Clear(request.ClearAlbumRoutingCache, request.ClearPhotoRoutingCache, request.ClearApiResponseCache);

            return Ok(result);
        }
    }
}
