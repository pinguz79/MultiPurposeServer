using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiPurposeServer.Shared.Contracts.Enums;
using MultiPurposeServer.Shared.Utils.Extensions;
using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Bulk.Responses;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Controllers.BackEnd.Bulk
{
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

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] BulkUpdateAlbumRequest request)
        {
            if (request.Options.ErrorStrategy != BulkErrorStrategy.WarningAndContinue)
            {
                return BadRequest("The requested error strategy is not supported.");
            }

            var warnings = new List<BulkUpdateAlbumWarning>();
            var updatedAlbums = new List<AlbumDto>();
            foreach (var item in request.Items)
            {
                try
                {

                    Album? album = null;
                    var name = Normalize(item.Name);
                    var description = Normalize(item.Description);

                    if (name is null && description is null)
                    {
                        warnings.Add(new BulkUpdateAlbumWarning(item.Id, "At least one field must be specified."));
                        continue;
                    }

                    await using var operation = await albumService.BeginOperation();
                    album = name is null ? album : await albumService.UpdateName(item.Id, name);
                    album = description is null ? album : await albumService.UpdateDescription(item.Id, description);
                    await operation.Complete();

                    updatedAlbums.Add(new AlbumDto(album!));
                }
                catch (KeyNotFoundException)
                {
                    warnings.Add(new BulkUpdateAlbumWarning(item.Id, "Album not found."));
                }
            }
            return Ok(new BulkUpdateAlbumResponse
            {
                UpdatedItems = updatedAlbums,
                Warnings = warnings
            });
        }
    }
}