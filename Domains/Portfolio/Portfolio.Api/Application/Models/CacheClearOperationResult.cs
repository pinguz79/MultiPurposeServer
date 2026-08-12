namespace Portfolio.Api.Application.Models
{
    public sealed class CacheClearOperationResult
    {
        public int AlbumRoutingEntriesDeleted { get; init; }
        public int PhotoRoutingEntriesDeleted { get; init; }
        public int ApiResponseEntriesDeleted { get; init; }
    }
}
