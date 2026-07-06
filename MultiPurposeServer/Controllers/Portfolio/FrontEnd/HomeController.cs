using Microsoft.AspNetCore.Mvc;
using MultiPurposeServer.Microservices.Portfolio;
using MultiPurposeServer.Models.Portfolio;

namespace MultiPurposeServer.Controllers.Portfolio.FrontEnd
{
    [Route("Portfolio/FrontEnd/[controller]")]
    [ApiController]
    public class HomeController(IAlbumService albumService) : PortfolioControllerBase(albumService)
    {
        [HttpGet("Albums")]
        public async Task<IActionResult> GetAlbums([FromQuery] Guid? id = null)
        {
            List<Album> albums = await albumService.GetAlbums(id);
            return Ok(albums);
        }
    }
}
