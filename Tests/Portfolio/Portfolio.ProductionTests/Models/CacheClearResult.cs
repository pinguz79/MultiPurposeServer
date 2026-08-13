namespace Portfolio.ProductionTests
{
    internal sealed record CacheClearResult(
        int AlbumRoutingEntriesDeleted,
        int PhotoRoutingEntriesDeleted,
        int ApiResponseEntriesDeleted);
}
