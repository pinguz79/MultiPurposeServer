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
    }
}