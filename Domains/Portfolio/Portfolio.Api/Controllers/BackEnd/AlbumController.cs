using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Controllers.BackEnd
{
    [Route("Portfolio/BackEnd/[controller]")]
    [ApiController]
    public class AlbumController(IAlbumService albumService, ILogger<AlbumController> logger) : PortfolioBackEndControllerBase(logger)
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList([FromQuery] Guid? id = null)
        {
            List<AlbumDto> albums = (await albumService.GetAlbums(id)).Select(album => new AlbumDto(album)).ToList();
            return Ok(albums);
        }

        [HttpGet("{albumId:guid}")]
        public async Task<IActionResult> Get(Guid albumId)
        {
            var album = await albumService.GetById(albumId);

            return album is null ? NotFound() : Ok(new AlbumDto(album));
        }

        [HttpPost("CreateNew")]
        public async Task<IActionResult> Create([FromBody] CreateAlbumRequest request)
        {
            var album = await albumService.CreateAlbum(request.Name, request.Parent, request.Description);
            var dto = new AlbumDto(album);

            return CreatedAtAction(nameof(Get), new { albumId = dto.Id }, dto);
        }

        [HttpPut("{albumId:guid}")]
        public async Task<IActionResult> Update(Guid albumId, [FromBody] UpdateAlbumRequest request)
        {
            try
            {
                await using var operation = await albumService.BeginOperation();

                Album? album = null;
                album = request.Name is null ? album : await albumService.UpdateName(albumId, request.Name);
                album = request.Description is null ? album : await albumService.UpdateDescription(albumId, request.Description);

                await operation.Complete();

                return Ok(new AlbumDto(album!));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}