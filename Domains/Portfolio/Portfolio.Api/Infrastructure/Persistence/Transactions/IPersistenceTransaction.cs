namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public interface IPersistenceTransaction : IAsyncDisposable
    {
        Task<IPersistenceCheckpoint> BeginCheckpoint();
        Task Commit();
    }
}
