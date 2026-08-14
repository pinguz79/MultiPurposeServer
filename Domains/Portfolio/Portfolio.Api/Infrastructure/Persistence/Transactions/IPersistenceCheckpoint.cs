namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public interface IPersistenceCheckpoint : IAsyncDisposable
    {
        Task Complete();
    }
}
