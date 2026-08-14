using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Shared.Logging.Diagnostics
{
    public static class SharedLoggingLogEvents
    {
        public static LogEventId DiagnosticsExpired { get; } = new("Shared.Logging.DiagnosticsExpired");
    }
}
