namespace Portfolio.Api.Infrastructure.Persistence.Transactions
{
    public sealed class PersistenceCheckpoint(ITransactionalRepository repository, string name) : IPersistenceCheckpoint
    {
        private bool _completed;
        private bool _disposed;

        public async Task Complete()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_completed)
            {
                return;
            }

            await repository.CompleteCheckpoint(name);
            _completed = true;
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
                    await repository.RollbackCheckpoint(name);
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
