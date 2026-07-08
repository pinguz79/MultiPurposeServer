using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Services;
using Portfolio.Contracts;

namespace Portfolio.Api.Controllers.FrontEnd;

[Route("Portfolio/FrontEnd/[controller]")]
[ApiController]
public class HomeController(IAlbumService albumService) : PortfolioControllerBase(albumService)
{
    [HttpGet("Albums")]
    public async Task<IActionResult> GetAlbums([FromQuery] Guid? id = null)
    {
        List<AlbumDto> albums = (await albumService.GetAlbums(id)).Select(album => new AlbumDto(album)).ToList();
        return Ok(albums);
    }
}
