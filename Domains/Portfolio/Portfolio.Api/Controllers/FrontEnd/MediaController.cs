using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Services;

namespace Portfolio.Api.Controllers.FrontEnd
{
    [Route("Portfolio/FrontEnd/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MediaController(IMediaService mediaService) : PortfolioFrontEndControllerBase
    {
        private const string CacheControlValue = "public, max-age=864000";

        [HttpGet("Cover/{photoId:guid}")]
        public Task<IActionResult> GetCover(Guid photoId) => GetMedia(photoId, mediaService.GetCoverPhoto);

        [HttpGet("EditorialCover/{photoId:guid}")]
        public Task<IActionResult> GetEditorialCover(Guid photoId) => GetMedia(photoId, mediaService.GetEditorialCoverPhoto);

        [HttpGet("Thumbnail/{photoId:guid}")]
        public Task<IActionResult> GetThumbnail(Guid photoId) => GetMedia(photoId, mediaService.GetThumbnailPhoto);

        [HttpGet("Image/{photoId:guid}")]
        public Task<IActionResult> GetImage(Guid photoId) => GetMedia(photoId, mediaService.GetImagePhoto);

        private async Task<IActionResult> GetMedia(Guid photoId, Func<Guid, Task<MediaFile?>> getMedia)
        {
            var media = await getMedia(photoId);

            if (media is null)
            {
                return NotFound();
            }

            Response.Headers.CacheControl = CacheControlValue;

            return PhysicalFile(media.FilePath, media.ContentType);
        }
    }
}
