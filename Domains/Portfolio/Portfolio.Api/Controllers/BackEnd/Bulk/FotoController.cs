using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Services;
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
    public async Task<IActionResult> UpdateDescriptions([FromBody] BulkUpdateAlbumNameRequest request)
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

        List<Foto>? fotos = await fotoService.BulkUpdateDescriptions(request.Items);

        if (fotos is null)
        {
            return NotFound("One or more fotos do not exist.");
        }

        return Ok(fotos.Select(foto => new PhotoDto(foto)).ToList());
    }
}