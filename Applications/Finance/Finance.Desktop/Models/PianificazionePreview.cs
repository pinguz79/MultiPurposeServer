namespace Finance.Desktop.Models
{
    public record PianificazionePreview(
        string Formula,
        string Description,
        int OccurrenceCount,
        bool IsValid,
        IReadOnlyList<string> Errors,
        IReadOnlyList<FormulaDependency> Dependencies,
        IReadOnlyList<PianificazioneOccurrence> Occurrences);
}
