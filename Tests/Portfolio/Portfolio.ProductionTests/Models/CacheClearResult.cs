namespace Portfolio.ProductionTests.Models
{
    internal sealed record CacheClearResult(
        int AlbumRoutingEntriesDeleted,
        int PhotoRoutingEntriesDeleted,
        int ApiResponseEntriesDeleted);
}
