using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Services;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Controllers.BackEnd;

[Route("Portfolio/BackEnd/[controller]")]
[ApiController]
public class FotoController(IFotoService fotoService, ILogger<FotoController> logger) : PortfolioControllerBase(logger)
{
    [HttpGet("List")]
    public async Task<IActionResult> GetList([FromQuery] Guid albumId)
    {
        List<PhotoDto> photos = (await fotoService.GetByAlbum(albumId)).Select(photo => new PhotoDto(photo)).ToList();

        return Ok(photos);
    }

    [HttpGet("{photoId:guid}")]
    public async Task<IActionResult> Get(Guid photoId)
    {
        var photo = await fotoService.GetById(photoId);

        return photo is null ? NotFound() : Ok(new PhotoDto(photo));
    }

    [HttpPut("{photoId:guid}")]
    public async Task<IActionResult> Update(Guid photoId, [FromBody] UpdatePhotoRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var photo = await fotoService.UpdateDescription(photoId, request.Description);

        return photo is null ? NotFound() : Ok(new PhotoDto(photo));
    }
}