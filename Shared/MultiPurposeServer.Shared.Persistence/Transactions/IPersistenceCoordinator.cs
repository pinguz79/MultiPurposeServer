namespace MultiPurposeServer.Shared.Persistence.Transactions
{
    public interface IPersistenceCoordinator : ITransactionalPersistence
    {
        Task<IPersistenceTransaction> BeginTransaction();
    }
}
