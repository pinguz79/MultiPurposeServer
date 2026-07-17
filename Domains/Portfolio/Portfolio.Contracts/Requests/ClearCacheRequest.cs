namespace Portfolio.Contracts.Requests
{
    public sealed record ClearCacheRequest(bool ClearAlbumRoutingCache, bool ClearPhotoRoutingCache, bool ClearApiResponseCache);
}
