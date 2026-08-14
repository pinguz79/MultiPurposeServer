namespace Portfolio.Contracts.Responses
{
    public sealed record LoggingDiagnosticStateDto(string Domain, string Mode, DateTimeOffset? ExpiresAt);
}
