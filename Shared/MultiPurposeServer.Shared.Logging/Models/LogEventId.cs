namespace MultiPurposeServer.Shared.Logging.Models
{
    public readonly record struct LogEventId(string Value)
    {
        public override string ToString() => Value;
    }
}
