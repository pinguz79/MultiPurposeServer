namespace Finance.Contracts.Responses
{
    public sealed record CreatePianificazioneDto(
        Guid Id,
        string Formula,
        string Description,
        int OccurrenceCount,
        bool IsValid,
        IReadOnlyList<string> Errors,
        IReadOnlyList<FormulaDependencyDto> Dependencies,
        IReadOnlyList<PianificazioneOccurrenceDto> Occurrences,
        IReadOnlyList<Guid> MovimentoIds);
}
