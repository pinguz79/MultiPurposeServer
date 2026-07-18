namespace Portfolio.Contracts.Responses
{
    public class CacheClearResult
    {
        public int AlbumRoutingEntriesDeleted { get; set; }
        public int PhotoRoutingEntriesDeleted { get; set; }
        public int ApiResponseEntriesDeleted { get; set; }
    }
}
