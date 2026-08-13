using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Services;

namespace Portfolio.Api.Controllers.FrontEnd
{
    [Route("Portfolio/FrontEnd/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MediaController(IMediaService mediaService, ILogger<MediaController> logger)
    : PortfolioFrontEndControllerBase(logger)
    {
        private const string CacheControlValue = "public, max-age=864000";

        #region Gestione risultato

        [HttpGet("Cover/{photoId:guid}")]
        public Task<IActionResult> GetCover(Guid photoId) => GetMedia(photoId, mediaService.GetCoverPhoto, "Errore nella generazione della cover");

        [HttpGet("EditorialCover/{photoId:guid}")]
        public Task<IActionResult> GetEditorialCover(Guid photoId) => GetMedia(photoId, mediaService.GetEditorialCoverPhoto, "Errore nella generazione della cover editoriale");

        #endregion

        [HttpGet("Thumbnail/{photoId:guid}")]
        public Task<IActionResult> GetThumbnail(Guid photoId) => GetMedia(photoId, mediaService.GetThumbnailPhoto, "Errore nella generazione della miniatura");


        #region Immagine originale

        [HttpGet("Image/{photoId:guid}")]
        public Task<IActionResult> GetImage(Guid photoId) => GetMedia(photoId, mediaService.GetImagePhoto, "Errore nella generazione dell'immagine");
        private async Task<IActionResult> GetMedia(Guid photoId, Func<Guid, Task<MediaFile?>> getMedia, string errorMessage)
        {
            try
            {
                var media = await getMedia(photoId);

                if (media is null)
                {
                    return NotFound();
                }

                Response.Headers.CacheControl = CacheControlValue;

                return PhysicalFile(media.FilePath, media.ContentType);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "{ErrorMessage} per la foto {PhotoId}", errorMessage, photoId);

                return Problem(title: errorMessage, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

    }
}
