using Portfolio.Contracts.Enums;

namespace Portfolio.Contracts.Requests
{
    public sealed record EnableLoggingDiagnosticRequest(LoggingDiagnosticMode Mode, int DurationMinutes);
}
