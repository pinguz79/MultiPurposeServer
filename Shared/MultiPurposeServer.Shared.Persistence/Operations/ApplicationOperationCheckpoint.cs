using MultiPurposeServer.Shared.Persistence.Transactions;

namespace MultiPurposeServer.Shared.Persistence.Operations
{
    public sealed class ApplicationOperationCheckpoint(IPersistenceCheckpoint checkpoint) : IApplicationOperationCheckpoint
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

            await checkpoint.Complete();
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
                await checkpoint.DisposeAsync();
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
