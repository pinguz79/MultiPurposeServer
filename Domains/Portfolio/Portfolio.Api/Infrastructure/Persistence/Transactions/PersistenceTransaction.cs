namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public sealed class PersistenceTransaction(ITransactionalRepository repository) : IPersistenceTransaction
    {
        private bool _completed;
        private bool _disposed;

        public async Task Commit()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_completed)
            {
                return;
            }

            try
            {
                await repository.CommitTransaction();
            }
            finally
            {
                _completed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (!_completed)
                {
                    await repository.RollbackTransaction();
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
