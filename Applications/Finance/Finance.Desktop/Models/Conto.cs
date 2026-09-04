namespace Finance.Desktop.Models
{
    public sealed record Conto(
        Guid Id,
        string Name,
        string DisplayName,
        decimal Balance,
        DateOnly? FirstNegativeBalanceDate = null,
        decimal? FirstNegativeBalance = null,
        CycleIndicators? CycleIndicators = null);
}
