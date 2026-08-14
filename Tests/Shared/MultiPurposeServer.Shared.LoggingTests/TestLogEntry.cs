using Microsoft.Extensions.Logging;

namespace MultiPurposeServer.Shared.LoggingTests
{
    internal sealed record TestLogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception, IReadOnlyDictionary<string, object?> Scope);
}
