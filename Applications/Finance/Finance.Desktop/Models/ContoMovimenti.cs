namespace Finance.Desktop.Models
{
    public sealed record ContoMovimenti(
        Conto Conto,
        int SelectedMonth,
        int SelectedYear,
        DateOnly From,
        DateOnly To,
        decimal OpeningBalance,
        decimal ClosingBalance,
        IReadOnlyList<Movimento> Items);
}
