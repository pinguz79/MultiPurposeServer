namespace MultiPurposeServer.Shared.Persistence.Transactions
{
    public sealed class PersistenceCheckpoint(ITransactionalPersistence persistence, string name) : IPersistenceCheckpoint
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

            await persistence.CompleteCheckpoint(name);
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
                    await persistence.RollbackCheckpoint(name);
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
