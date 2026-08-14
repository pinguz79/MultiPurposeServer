namespace MultiPurposeServer.Shared.Logging.Models
{
    public sealed class DiagnosticOptions
    {
        public TimeSpan MaximumDiagnosticDuration { get; init; } = TimeSpan.FromHours(1);

        public TimeSpan MaximumVerboseDuration { get; init; } = TimeSpan.FromMinutes(15);
    }
}
