using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Application.Models;

namespace Portfolio.Api.Controllers.FrontEnd
{
    [Route("Portfolio/FrontEnd/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MediaController(IMediaService mediaService, ILogger<MediaController> logger) : PortfolioFrontEndControllerBase(logger)
    {
        private const string CacheControlValue = "public, max-age=864000";

        [HttpGet("Cover/{photoId:guid}")]
        public Task<IActionResult> GetCover(Guid photoId) => GetMedia(photoId, mediaService.GetCoverPhoto, "Errore nella generazione della cover");

        [HttpGet("Thumbnail/{photoId:guid}")]
        public Task<IActionResult> GetThumbnail(Guid photoId) => GetMedia(photoId, mediaService.GetThumbnailPhoto, "Errore nella generazione della miniatura");

        [HttpGet("Image/{photoId:guid}")]
        public Task<IActionResult> GetImage(Guid photoId) => GetMedia(photoId, mediaService.GetImagePhoto, "Errore nella generazione dell'immagine");
        private async Task<IActionResult> GetMedia(Guid photoId, Func<Guid, Task<MediaFile?>> getMedia, string errorMessage)
        {
            try
            {
                var media = await getMedia(photoId);

                if (media == null)
                {
                    return NotFound();
                }

                Response.Headers.CacheControl = CacheControlValue;

                return PhysicalFile(media.FilePath, media.ContentType);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"{errorMessage} per la foto {photoId}");

                return Problem(
                    title: errorMessage,
                    detail: exception.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}