namespace Portfolio.Contracts.Requests
{
    public sealed record CacheClearRequest(bool ClearAlbumRoutingCache, bool ClearPhotoRoutingCache, bool ClearApiResponseCache);
}
