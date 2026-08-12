namespace MultiPurposeServer.Shared.Contracts.Abstractions
{
    public interface IBulk<TItem>
    {
        BulkOptions Options { get; }
        IReadOnlyCollection<TItem> Items { get; }
    }
}
