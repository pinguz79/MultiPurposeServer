using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Diagnostics
{
    public static class HostLogEvents
    {
        public static LogEventId UnhandledHttpException { get; } = new("Host.Http.UnhandledException");
    }
}
