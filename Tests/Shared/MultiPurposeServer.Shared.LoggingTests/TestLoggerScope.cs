namespace MultiPurposeServer.Shared.LoggingTests
{
    internal sealed class TestLoggerScope(Action restore) : IDisposable
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
