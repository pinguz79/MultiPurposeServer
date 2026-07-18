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
public class FotoController(IFotoService fotoService, ILogger<FotoController> logger) : PortfolioBackEndControllerBase(logger)
{
    [HttpGet("MissingDescriptions")]
    public async Task<IActionResult> MissingDescriptions()
    {
        try
        {
            List<FotoMissingDescriptionsDto> result = (await fotoService.GetMissingDescriptions())
                .Select(foto => new FotoMissingDescriptionsDto(foto))
                .ToList();

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("Descriptions")]
    public async Task<IActionResult> UpdateDescriptions([FromBody] BulkUpdateFotoDescriptionRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one photo is required.");
        }

        if (request.Items.Any(item => string.IsNullOrWhiteSpace(item.NewDescription)))
        {
            return BadRequest("Every photo must have a valid new description.");
        }

        if (request.Items.GroupBy(item => item.Id).Any(group => group.Count() > 1))
        {
            return BadRequest("The request contains duplicate photo ids.");
        }

        var items = request.Items.Select(item => new BulkUpdateItem<string>(item.Id, item.NewDescription)).ToList();
        var photos = await fotoService.BulkUpdateDescriptions(items);

        if (photos is null)
        {
            return NotFound("One or more photos do not exist.");
        }

        return Ok(photos.Select(photo => new PhotoDto(photo)).ToList());
    }
}