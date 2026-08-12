namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public interface ITransactionalRepository
    {
        Task CommitTransaction();
        Task RollbackTransaction();
    }
}
