using MultiPurposeServer.Shared.Logging.Abstractions;
using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Shared.Logging.Services
{
    public sealed class LoggingContextAccessor : ILoggingContextAccessor
    {
        private readonly AsyncLocal<LoggingContext?> _current = new();

        public LoggingContext Current => _current.Value ?? LoggingContext.Empty;

        public IDisposable Push(LoggingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var previous = _current.Value;
            _current.Value = context;
            return new LoggingContextScope(() => _current.Value = previous);
        }
    }
}
