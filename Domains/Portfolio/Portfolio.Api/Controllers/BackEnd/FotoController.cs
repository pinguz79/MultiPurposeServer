using Microsoft.AspNetCore.Mvc;

using Portfolio.Api.Application.Services;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Controllers.BackEnd
{
    [Route("Portfolio/BackEnd/[controller]")]
    [ApiController]
    public class FotoController(
    IFotoService fotoService,
    ICacheService cacheService) : PortfolioBackEndControllerBase
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList([FromQuery] Guid albumId)
        {
            List<PhotoDto> photos = [.. (await fotoService.GetByAlbum(albumId)).Select(photo => new PhotoDto(photo))];

            return Ok(photos);
        }

        [HttpGet("{photoId:guid}")]
        public async Task<IActionResult> Get(Guid photoId)
        {
            var photo = await fotoService.GetById(photoId);

            return photo is null ? NotFound() : Ok(new PhotoDto(photo));
        }

        [HttpPut("{photoId:guid}")]
        public async Task<IActionResult> Update(Guid photoId, [FromBody] UpdatePhotoRequest request)
        {
            await using var operation = await fotoService.BeginOperation();

            Foto? photo = null;
            photo = request.Description is null ? photo : await fotoService.UpdateDescription(photoId, request.Description);
            photo = request.ContentRating is null ? photo : await fotoService.UpdateContentRating(photoId, request.ContentRating.Value);

            await operation.Complete();

            if (request.ContentRating is not null)
            {
                await cacheService.Clear(clearAlbumRoutingCache: true, clearPhotoRoutingCache: false, clearApiResponseCache: true);
            }

            return Ok(new PhotoDto(photo!));
        }
    }
}
