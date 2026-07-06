using Microsoft.AspNetCore.Mvc;
using MultiPurposeServer.Microservices.Portfolio;
using MultiPurposeServer.Models.Portfolio;
using Portfolio.Contracts;

namespace MultiPurposeServer.Controllers.Portfolio.BackEnd
{
    [Route("Portfolio/BackEnd/[controller]")]
    [ApiController]
    public class AlbumController(IAlbumService albumService) : PortfolioControllerBase(albumService)
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList([FromQuery] Guid? id = null)
        {
            List<Album> albums = await albumService.GetAlbums(id);
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

            Album album = await albumService.CreateAlbum(albumRequest.Name, albumRequest.Parent);

            return Created($"Portfolio/BackEnd/Album/{album.Id}", album);
        }
    }
}
