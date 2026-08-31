namespace Finance.Contracts.Responses
{
    public sealed record PianificazioneOccurrenceDto(
        DateOnly Date,
        decimal? Value,
        string Status,
        IReadOnlyList<string> Messages);
}
