namespace MultiPurposeServer.Shared.Models
{
    public class PagedResult<T>(IEnumerable<T> items, int totalItems)
    {
        public IReadOnlyList<T> Items => [.. items];
        public int TotalItems => totalItems;
    }
}
