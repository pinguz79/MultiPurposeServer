using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Portfolio.Contracts.Requests
{
    public sealed record CacheClearRequest([property: RequiredAtLeastOneTrue] bool ClearAlbumRoutingCache, [property: RequiredAtLeastOneTrue] bool ClearPhotoRoutingCache, [property: RequiredAtLeastOneTrue] bool ClearApiResponseCache) : IRequest
    {
    }
}
