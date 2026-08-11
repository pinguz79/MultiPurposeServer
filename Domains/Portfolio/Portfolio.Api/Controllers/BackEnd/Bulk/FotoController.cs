using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiPurposeServer.Shared.Contracts.Enums;
using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Bulk.Responses;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Controllers.BackEnd.Bulk
{
    [Route("Portfolio/BackEnd/Bulk/[controller]")]
    [ApiController]
    public class FotoController(IFotoService fotoService, ICacheService cacheService, ILogger<FotoController> logger) : PortfolioBackEndControllerBase(logger)
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
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] BulkUpdateFotoRequest request)
        {
            if (request.Options.ErrorStrategy != BulkErrorStrategy.WarningAndContinue)
            {
                return BadRequest("The requested error strategy is not supported.");
            }

            var warnings = new List<BulkUpdateFotoWarning>();
            var updatedPhotos = new List<PhotoDto>();
            var contentRatingChanged = false;
            foreach (var item in request.Items)
            {
                try
                {
                    Foto? photo = null;

                    await using var operation = await fotoService.BeginOperation();
                    photo = item.Description is null ? photo : await fotoService.UpdateDescription(item.Id, item.Description);
                    photo = item.ContentRating is null ? photo : await fotoService.UpdateContentRating(item.Id, item.ContentRating.Value);
                    await operation.Complete();

                    contentRatingChanged |= item.ContentRating is not null;

                    updatedPhotos.Add(new PhotoDto(photo!));
                }
                catch (KeyNotFoundException)
                {
                    warnings.Add(new BulkUpdateFotoWarning(item.Id, "Photo not found."));
                }
            }

            if (contentRatingChanged)
            {
                await cacheService.Clear(clearAlbumRoutingCache: true, clearPhotoRoutingCache: false, clearApiResponseCache: true);
            }

            return Ok(new BulkUpdateFotoResponse
            {
                UpdatedItems = updatedPhotos,
                Warnings = warnings
            });
        }
    }
}
