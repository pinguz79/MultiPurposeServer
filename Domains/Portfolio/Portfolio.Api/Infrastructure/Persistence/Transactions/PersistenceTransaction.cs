namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public sealed class PersistenceTransaction(ITransactionalRepository repository)
        : IPersistenceTransaction
    {
        private int _checkpointCounter;
        private bool _completed;
        private bool _disposed;

        public async Task<IPersistenceCheckpoint> BeginCheckpoint()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var name = $"BulkItem{++_checkpointCounter}";
            await repository.CreateCheckpoint(name);

            return new PersistenceCheckpoint(repository, name);
        }

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
