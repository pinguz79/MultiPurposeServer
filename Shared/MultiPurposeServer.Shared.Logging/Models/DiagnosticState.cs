namespace MultiPurposeServer.Shared.Logging.Models
{
    public sealed record DiagnosticState(string Domain, DiagnosticMode Mode, DateTimeOffset? ExpiresAt)
    {
        public bool IsEnabled => Mode != DiagnosticMode.Off;

        public static DiagnosticState Off(string domain) => new(domain, DiagnosticMode.Off, null);
    }
}
