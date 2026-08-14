using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Shared.Logging.Abstractions
{
    public interface ILoggerService<T>
    {
        void Trace(LogEventId eventId, string message, params object?[] args);

        void Debug(LogEventId eventId, string message, params object?[] args);

        void Information(LogEventId eventId, string message, params object?[] args);

        void Warning(LogEventId eventId, string message, params object?[] args);

        void Error(LogEventId eventId, Exception exception, string message, params object?[] args);

        void Critical(LogEventId eventId, Exception exception, string message, params object?[] args);
    }
}
