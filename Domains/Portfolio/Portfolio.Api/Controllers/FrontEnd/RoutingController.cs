using Microsoft.AspNetCore.Mvc;

using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Controllers.FrontEnd
{
    [Route("Portfolio/FrontEnd/[controller]")]
    [ApiController]
    public class RoutingController(IAlbumService albumService) : PortfolioFrontEndControllerBase
    {
        [HttpGet("Album")]
        public async Task<IActionResult> ResolveAlbumPath([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest();
            }

            var album = await albumService.ResolvePath(path);
            return album == null ? NotFound() : Ok(new AlbumDto(album));
        }
    }
}
