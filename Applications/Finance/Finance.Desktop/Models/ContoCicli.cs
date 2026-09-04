namespace Finance.Desktop.Models
{
    public sealed record ContoCicli(
        Conto Conto,
        int SelectedMonth,
        int SelectedYear,
        DateOnly From,
        DateOnly To,
        decimal OpeningBalance,
        decimal ClosingBalance,
        IReadOnlyList<Ciclo> Cycles);
}
