using Microsoft.Extensions.Logging;

using MultiPurposeServer.Shared.Logging.Abstractions;
using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Shared.Logging.Services
{
    public sealed class LoggerService<T>(ILogger<T> logger, ILoggingContextAccessor contextAccessor, IDiagnosticStateRegistry diagnosticStateRegistry) : ILoggerService<T>
    {
        public void Trace(LogEventId eventId, string message, params object?[] args) => Write(LogLevel.Trace, eventId, null, message, args);

        public void Debug(LogEventId eventId, string message, params object?[] args) => Write(LogLevel.Debug, eventId, null, message, args);

        public void Information(LogEventId eventId, string message, params object?[] args) => Write(LogLevel.Information, eventId, null, message, args);

        public void Warning(LogEventId eventId, string message, params object?[] args) => Write(LogLevel.Warning, eventId, null, message, args);

        public void Error(LogEventId eventId, Exception exception, string message, params object?[] args) => Write(LogLevel.Error, eventId, exception, message, args);

        public void Critical(LogEventId eventId, Exception exception, string message, params object?[] args) => Write(LogLevel.Critical, eventId, exception, message, args);

        private void Write(LogLevel originalLevel, LogEventId eventId, Exception? exception, string message, params object?[] args)
        {
            var context = contextAccessor.Current;
            var mode = diagnosticStateRegistry.Get(context.Domain).Mode;
            var effectiveLevel = GetEffectiveLevel(originalLevel, mode);
            if (effectiveLevel is null)
            {
                return;
            }

            var scope = new Dictionary<string, object?>
            {
                ["Domain"] = context.Domain,
                ["CorrelationId"] = context.CorrelationId,
                ["RequestId"] = context.RequestId,
                ["Origin"] = context.Origin,
                ["OriginalLevel"] = originalLevel.ToString(),
                ["IsDiagnostic"] = originalLevel is LogLevel.Debug or LogLevel.Trace,
                ["DiagnosticMode"] = mode.ToString(),
            };

            using (logger.BeginScope(scope))
            {
                logger.Log(effectiveLevel.Value, new EventId(0, eventId.Value), exception, message, args);
            }
        }

        private static LogLevel? GetEffectiveLevel(LogLevel originalLevel, DiagnosticMode mode) => originalLevel switch
        {
            LogLevel.Trace => mode == DiagnosticMode.Verbose ? LogLevel.Information : null,
            LogLevel.Debug => mode is DiagnosticMode.Diagnostic or DiagnosticMode.Verbose ? LogLevel.Information : null,
            _ => originalLevel,
        };
    }
}
