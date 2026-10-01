namespace Finance.DataModel.Models
{
    public sealed record PianoFinanziamento(DateOnly ReferenceDate, decimal RemainingPrincipal, int RemainingInstallments, decimal RemainingTotal,
        DateOnly LastDueDate, decimal FinalPrincipal, decimal TotalDiscrepancy, RataFinanziamento? LastAlignment, IReadOnlyList<RataFinanziamento> Installments);
}
