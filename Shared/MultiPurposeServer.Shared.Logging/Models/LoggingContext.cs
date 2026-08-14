namespace MultiPurposeServer.Shared.Logging.Models
{
    public sealed record LoggingContext(string Domain, string? CorrelationId = null, string? RequestId = null, string Origin = "Server")
    {
        public static LoggingContext Empty { get; } = new("Unknown");
    }
}
