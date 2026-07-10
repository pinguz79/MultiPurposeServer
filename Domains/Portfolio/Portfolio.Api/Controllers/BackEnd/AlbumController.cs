using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Services;
using Portfolio.Contracts;

namespace Portfolio.Api.Controllers.BackEnd;

[Route("Portfolio/BackEnd/[controller]")]
[ApiController]
public class AlbumController(IAlbumService albumService, ILogger<AlbumController> logger) : PortfolioControllerBase(logger)
{
    [HttpGet("List")]
    public async Task<IActionResult> GetList([FromQuery] Guid? id = null)
    {
        List<AlbumDto> albums = (await albumService.GetAlbums(id)).Select(album => new AlbumDto(album)).ToList();
        return Ok(albums);
    }

    [HttpPost("CreateNew")]
    public async Task<IActionResult> Create([FromBody] CreateAlbumRequest albumRequest)
    {
        var existing = await albumService.GetAlbums(albumRequest.Parent);

        if (existing is null)
        {
            return BadRequest("Parent album does not exist.");
        }

        if (existing.Any(a => a.Name == albumRequest.Name))
        {
            return BadRequest("Album with the same name already exists.");
        }

        AlbumDto album = new AlbumDto(await albumService.CreateAlbum(albumRequest.Name, albumRequest.Parent));

        return Created($"Portfolio/BackEnd/Album/{album.Id}", album);
    }
}
