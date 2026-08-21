namespace MultiPurposeServer.Shared.Persistence.Transactions
{
    public interface IPersistenceTransaction : IAsyncDisposable
    {
        Task<IPersistenceCheckpoint> BeginCheckpoint();
        Task Commit();
    }
}
