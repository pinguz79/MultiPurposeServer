namespace MultiPurposeServer.Shared.Persistence.Transactions
{
    public interface IPersistenceCheckpoint : IAsyncDisposable
    {
        Task Complete();
    }
}
