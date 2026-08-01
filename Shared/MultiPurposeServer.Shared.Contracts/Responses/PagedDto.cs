namespace MultiPurposeServer.Shared.Contracts.Responses
{
    public class PageDto<T>(IEnumerable<T> items, int page, int pageSize, int totalItems)
    {
        public IReadOnlyList<T> Items => [.. items];
        public int Page => page;
        public int PageSize => pageSize;
        public int TotalItems => totalItems;
        public int TotalPages => TotalItems == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}