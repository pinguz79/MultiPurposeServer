namespace MultiPurposeServer.Shared.Persistence.Transactions
{
    public interface ITransactionalPersistence
    {
        bool IsTransactionActive { get; }

        Task CreateCheckpoint(string name);
        Task CompleteCheckpoint(string name);
        Task CommitTransaction();
        Task RollbackCheckpoint(string name);
        Task RollbackTransaction();
    }
}
