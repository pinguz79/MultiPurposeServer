namespace Finance.Desktop.Models
{
    public sealed record PianoFinanziamento(Finanziamento Finanziamento, DateOnly ReferenceDate, decimal RemainingPrincipal, int RemainingInstallments,
        decimal RemainingTotal, DateOnly LastDueDate, decimal FinalPrincipal, decimal TotalDiscrepancy, RataFinanziamento? LastAlignment, IReadOnlyList<RataFinanziamento> Installments);
}
