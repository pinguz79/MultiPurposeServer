using Microsoft.Extensions.Logging;

namespace MultiPurposeServer.Shared.LoggingTests
{
    internal sealed class TestLogger<T> : ILogger<T>
    {
        private IReadOnlyDictionary<string, object?> _currentScope = new Dictionary<string, object?>();

        public List<TestLogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            var previous = _currentScope;
            _currentScope = state as IReadOnlyDictionary<string, object?> ?? new Dictionary<string, object?>();
            return new TestLoggerScope(() => _currentScope = previous);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add(new TestLogEntry(logLevel, eventId, formatter(state, exception), exception, new Dictionary<string, object?>(_currentScope)));
    }
}
