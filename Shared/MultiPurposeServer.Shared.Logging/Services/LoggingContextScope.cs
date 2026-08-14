namespace MultiPurposeServer.Shared.Logging.Services
{
    internal sealed class LoggingContextScope(Action restore) : IDisposable
    {
        private readonly Action _restore = restore;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _restore();
            _disposed = true;
        }
    }
}
