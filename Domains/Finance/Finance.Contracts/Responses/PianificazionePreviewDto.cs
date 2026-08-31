namespace Finance.Contracts.Responses
{
    public sealed record PianificazionePreviewDto(
        string Formula,
        string Description,
        int OccurrenceCount,
        bool IsValid,
        IReadOnlyList<string> Errors,
        IReadOnlyList<FormulaDependencyDto> Dependencies,
        IReadOnlyList<PianificazioneOccurrenceDto> Occurrences);
}
