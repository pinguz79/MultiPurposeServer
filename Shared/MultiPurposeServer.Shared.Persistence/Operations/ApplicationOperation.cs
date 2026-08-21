using MultiPurposeServer.Shared.Persistence.Transactions;

namespace MultiPurposeServer.Shared.Persistence.Operations
{
    public sealed class ApplicationOperation(IPersistenceTransaction transaction) : IApplicationOperation
    {
        private bool _completed;
        private bool _disposed;

        public async Task<IApplicationOperationCheckpoint> BeginCheckpoint()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return new ApplicationOperationCheckpoint(await transaction.BeginCheckpoint());
        }

        public async Task Complete()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_completed)
            {
                return;
            }

            await transaction.Commit();
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
                await transaction.DisposeAsync();
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
