namespace MultiPurposeServer.Shared.Persistence.Transactions
{
    public sealed class PersistenceTransaction(ITransactionalPersistence persistence) : IPersistenceTransaction
    {
        private int _checkpointCounter;
        private bool _completed;
        private bool _disposed;

        public async Task<IPersistenceCheckpoint> BeginCheckpoint()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var name = $"BulkItem{++_checkpointCounter}";
            await persistence.CreateCheckpoint(name);

            return new PersistenceCheckpoint(persistence, name);
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
                await persistence.CommitTransaction();
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
                    await persistence.RollbackTransaction();
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
