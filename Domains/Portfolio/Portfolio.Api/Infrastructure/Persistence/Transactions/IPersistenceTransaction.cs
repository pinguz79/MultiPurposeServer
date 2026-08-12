namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public interface IPersistenceTransaction : IAsyncDisposable
    {
        Task Commit();
    }
}
