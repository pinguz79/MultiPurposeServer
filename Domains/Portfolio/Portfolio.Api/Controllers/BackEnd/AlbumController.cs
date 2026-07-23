using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Controllers.BackEnd;

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
    public async Task<IActionResult> Create([FromBody] CreateAlbumRequest albumRequest)
    {
        if (string.IsNullOrWhiteSpace(albumRequest.Name))
        {
            return BadRequest("Album name is required.");
        }

        var album = await albumService.CreateAlbum(albumRequest.Name, albumRequest.Parent);
        var dto = new AlbumDto(album);

        return CreatedAtAction(nameof(Get), new { albumId = dto.Id }, dto);
    }

    [HttpPut("{albumId:guid}")]
    public async Task<IActionResult> Update(Guid albumId, [FromBody] UpdateAlbumRequest albumRequest)
    {
        var name = Normalize(albumRequest.Name);
        var description = Normalize(albumRequest.Description);

        if (name is null && description is null)
        {
            return BadRequest("At least one field must be specified.");
        }

        try
        {
            await using var operation = await albumService.BeginOperation();

            Album? album = null;
            album = name is null ? album : await albumService.UpdateName(albumId, name);
            album = description is null ? album : await albumService.UpdateDescription(albumId, description);

            await operation.Complete();

            return Ok(new AlbumDto(album!));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}