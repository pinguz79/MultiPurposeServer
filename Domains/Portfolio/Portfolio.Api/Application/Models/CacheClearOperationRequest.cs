namespace Portfolio.Api.Application.Models
{
    public sealed record CacheClearOperationRequest(bool ClearAlbumRoutingCache, bool ClearPhotoRoutingCache, bool ClearApiResponseCache);
}