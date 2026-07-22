namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public sealed class PersistenceTransaction(ITransactionalRepository repository) : IPersistenceTransaction
    {
        private bool _committed;
        private bool _disposed;

        public async Task Commit()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_committed)
            {
                return;
            }

            await repository.CommitTransaction();
            _committed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            if (!_committed)
            {
                await repository.RollbackTransaction();
            }

            _disposed = true;
        }
    }
}