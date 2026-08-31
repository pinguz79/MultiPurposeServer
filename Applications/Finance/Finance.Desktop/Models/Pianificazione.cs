namespace Finance.Desktop.Models
{
    public record Pianificazione(Guid Id, string Formula, string Description, int OccurrenceCount, IReadOnlyList<Guid> MovimentoIds);
}
