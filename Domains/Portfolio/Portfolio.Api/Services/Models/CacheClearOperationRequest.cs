namespace Portfolio.Api.Services.Models;

public sealed record CacheClearOperationRequest(bool ClearAlbumRoutingCache, bool ClearPhotoRoutingCache, bool ClearApiResponseCache);