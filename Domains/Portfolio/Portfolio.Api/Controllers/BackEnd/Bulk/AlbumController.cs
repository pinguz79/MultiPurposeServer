using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Contracts.Enums;
using MultiPurposeServer.Shared.Contracts.Responses;

using Portfolio.Api.Application.Bulk;
using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Bulk.Responses;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Controllers.BackEnd.Bulk
{
    [Route("Portfolio/BackEnd/Bulk/[controller]")]
    [ApiController]
    public class AlbumController(IAlbumService albumService) : PortfolioBackEndControllerBase
    {

        [HttpGet("MissingDescriptions")]
        public async Task<IActionResult> MissingDescriptions()
        {
            List<AlbumMissingDescriptionsDto> result = [.. (await albumService.GetMissingDescriptions())
                .Select(album => new AlbumMissingDescriptionsDto(album))
                .OrderBy(album => album.FullPath)];

            return Ok(result);
        }

        [HttpGet("Match")]
        public async Task<IActionResult> MatchNames([FromQuery] string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return BadRequest("Regex pattern is required.");
            }

            try
            {
                List<AlbumMatchDto> result = [.. (await albumService.GetByNamePattern(pattern)).Select(album => new AlbumMatchDto(album))];

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
            BulkResponse<Guid, AlbumDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.Id,
                async item =>
                {
                    Album? album = null;
                    album = item.Name is null ? album : await albumService.UpdateName(item.Id, item.Name);
                    album = item.Description is null ? album : await albumService.UpdateDescription(item.Id, item.Description);

                    return new AlbumDto(album!);
                },
                albumService.BeginOperation,
                exception => exception is KeyNotFoundException
                    ? new BulkError(BulkErrorKind.Persistence, "AlbumNotFound", "Album not found.")
                    : null);

            return Ok(response);
        }
    }
}
