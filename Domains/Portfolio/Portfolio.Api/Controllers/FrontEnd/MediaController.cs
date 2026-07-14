using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Services;

namespace Portfolio.Api.Controllers.FrontEnd
{
    [Route("Portfolio/FrontEnd/[controller]")]
    [ApiController]
    public class MediaController(IMediaService mediaService, ILogger<MediaController> logger) : PortfolioControllerBase(logger)
    {
        [HttpGet("Cover/{photoId:guid}")]
        public async Task<IActionResult> GetCover(Guid photoId)
        {
            try
            {
                var coverPhoto = await mediaService.GetCoverPhoto(photoId);

                if (coverPhoto == null)
                {
                    return NotFound();
                }

                Response.Headers.CacheControl = "public, max-age=864000";

                return PhysicalFile(coverPhoto.FilePath, coverPhoto.ContentType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Errore nella generazione della cover per la foto {photoId}");

                return Problem(title: "Errore nella generazione della cover",
                    detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("Thumbnail/{photoId:guid}")]
        public async Task<IActionResult> GetThumbnail(Guid photoId)
        {
            try
            {
                var thumbnailPhoto = await mediaService.GetThumbnailPhoto(photoId);
                if (thumbnailPhoto == null)
                {
                    return NotFound();
                }
                Response.Headers.CacheControl = "public, max-age=864000";
                return PhysicalFile(thumbnailPhoto.FilePath, thumbnailPhoto.ContentType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Errore nella generazione della miniatura per la foto {photoId}");
                return Problem(title: "Errore nella generazione della miniatura",
                    detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("Image/{photoId:guid}")]
        public async Task<IActionResult> GetImage(Guid photoId)
        {
            try
            {
                var imagePhoto = await mediaService.GetImagePhoto(photoId);

                if (imagePhoto == null)
                {
                    return NotFound();
                }

                Response.Headers.CacheControl = "public, max-age=864000";

                return PhysicalFile(imagePhoto.FilePath, imagePhoto.ContentType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Errore nella generazione dell'immagine per la foto {photoId}");

                return Problem(title: "Errore nella generazione dell'immagine",
                    detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}