using Portfolio.Api.Infrastructure.Persistence.Transactions;

namespace Portfolio.Api.Application.Operations
{
    public sealed class ApplicationOperation(IPersistenceTransaction transaction) : IApplicationOperation
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