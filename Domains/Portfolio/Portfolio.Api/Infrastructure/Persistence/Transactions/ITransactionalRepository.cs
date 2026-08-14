namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public interface ITransactionalRepository
    {
        Task CreateCheckpoint(string name);
        Task CompleteCheckpoint(string name);
        Task CommitTransaction();
        Task RollbackCheckpoint(string name);
        Task RollbackTransaction();
    }
}
