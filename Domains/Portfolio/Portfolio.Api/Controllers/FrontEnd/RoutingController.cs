using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Controllers.FrontEnd
{
    [Route("Portfolio/FrontEnd/[controller]")]
    [ApiController]
    public class RoutingController(IAlbumService albumService, ILogger<RoutingController> logger) : PortfolioFrontEndControllerBase(logger)
    {
        [HttpGet("Album")]
        public async Task<IActionResult> ResolveAlbumPath([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest();
            }

            var album = await albumService.ResolvePath(path);

            if (album == null)
            {
                return NotFound();
            }

            return Ok(new AlbumDto(album));
        }
    }
}
