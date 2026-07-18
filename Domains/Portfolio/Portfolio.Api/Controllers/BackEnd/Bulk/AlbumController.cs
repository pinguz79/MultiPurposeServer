using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Models;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Bulk.Responses;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Controllers.BackEnd.Bulk;

[Route("Portfolio/BackEnd/Bulk/[controller]")]
[ApiController]
public class AlbumController(IAlbumService albumService, ILogger<AlbumController> logger) : PortfolioBackEndControllerBase(logger)
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
            List<AlbumMatchDto> result = (await albumService.GetByNamePattern(pattern))
                .Select(album => new AlbumMatchDto(album))
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

        var items = request.Items.Select(item => new BulkUpdateItem<string>(item.Id, item.NewName)).ToList();
        var albums = await albumService.BulkUpdateNames(items);

        if (albums is null)
        {
            return NotFound("One or more albums do not exist.");
        }

        return Ok(albums.Select(album => new AlbumDto(album)).ToList());
    }
}