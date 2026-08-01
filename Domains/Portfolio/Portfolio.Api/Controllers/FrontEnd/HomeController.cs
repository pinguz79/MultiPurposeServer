using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Responses;
using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Controllers.FrontEnd;

[Route("Portfolio/FrontEnd/[controller]")]
[ApiController]
public class HomeController(IAlbumService albumService, IFotoService fotoService, ILogger<HomeController> logger) : PortfolioFrontEndControllerBase(logger)
{
    [HttpGet("Albums")]
    public async Task<IActionResult> GetAlbums([FromQuery] Guid? id = null) => Ok((await albumService.GetAlbums(id)).Select(album => new AlbumDto(album)).ToList());

    [HttpGet("Album/{albumId:guid}/Photos")]
    public async Task<IActionResult> GetAlbumPhotos(Guid albumId, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        page = Math.Max(page, 1);
        pageSize = pageSize switch
        {
            12 => 12,
            24 => 24,
            48 => 48,
            _ => 12
        };

        var result = await fotoService.GetByAlbumId(albumId, page, pageSize);
        var response = new PageDto<PhotoDto>(result.Items.Select(photo => new PhotoDto(photo)), page, pageSize, result.TotalItems);

        return Ok(response);
    }
}
