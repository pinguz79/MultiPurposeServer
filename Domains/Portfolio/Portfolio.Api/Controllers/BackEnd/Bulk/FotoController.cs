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
    public class FotoController(
    IFotoService fotoService,
    ICacheService cacheService) : PortfolioBackEndControllerBase
    {
        [HttpGet("MissingDescriptions")]
        public async Task<IActionResult> MissingDescriptions()
        {
            try
            {
                List<FotoMissingDescriptionsDto> result = [.. (await fotoService.GetMissingDescriptions()).Select(foto => new FotoMissingDescriptionsDto(foto))];

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] BulkUpdateFotoRequest request)
        {
            BulkResponse<Guid, PhotoDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.Id,
                async item =>
                {
                    Foto? photo = null;
                    photo = item.Description is null ? photo : await fotoService.UpdateDescription(item.Id, item.Description);
                    photo = item.ContentRating is null ? photo : await fotoService.UpdateContentRating(item.Id, item.ContentRating.Value);

                    return new PhotoDto(photo!);
                },
                fotoService.BeginOperation,
                exception => exception is KeyNotFoundException
                    ? new BulkError(BulkErrorKind.Persistence, "PhotoNotFound", "Photo not found.")
                    : null);

            if (response.Items.Any(result => result.Persisted && request.Items.ElementAt(result.Index).ContentRating is not null))
            {
                await cacheService.Clear(clearAlbumRoutingCache: true, clearPhotoRoutingCache: false, clearApiResponseCache: true);
            }

            return Ok(response);
        }
    }
}
