namespace Finance.Desktop.Models
{
    public record PianificazioneOccurrence(DateOnly Date, decimal? Value, string Status, IReadOnlyList<string> Messages);
}
