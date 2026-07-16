using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Services;
using Portfolio.Contracts;
using Portfolio.Contracts.Models.Bulk;
using Portfolio.Data.Models;

namespace Portfolio.Api.Controllers.BackEnd.Bulk;

[Route("Portfolio/BackEnd/Bulk/[controller]")]
[ApiController]
public class AlbumController(IAlbumService albumService, ILogger<AlbumController> logger) : PortfolioControllerBase(logger)
{
    [HttpGet("Match")]
    public async Task<IActionResult> MatchNames([FromQuery] string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return BadRequest("Regex pattern is required.");
        }

        try
        {
            List<AlbumNameMatchDto> result = (await albumService.GetByNamePattern(pattern))
                .Select(album => new AlbumNameMatchDto
                {
                    Id = album.Id,
                    Name = album.Name
                })
                .ToList();

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("Names")]
    public async Task<IActionResult> UpdateNames([FromBody] BulkUpdateAlbumNameRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one album is required.");
        }

        if (request.Items.Any(item => string.IsNullOrWhiteSpace(item.NewName)))
        {
            return BadRequest("Every album must have a valid new name.");
        }

        if (request.Items.GroupBy(item => item.Id).Any(group => group.Count() > 1))
        {
            return BadRequest("The request contains duplicate album ids.");
        }

        List<Album>? albums = await albumService.BulkUpdateNames(request.Items);

        if (albums is null)
        {
            return NotFound("One or more albums do not exist.");
        }

        return Ok(albums.Select(album => new AlbumDto(album)).ToList());
    }
}