namespace Finance.Desktop.Models
{
    public sealed record MovimentoCiclo(
        Guid Id,
        DateOnly Date,
        string Description,
        decimal Amount,
        decimal BalanceAfter,
        decimal CycleBalanceAfter);
}
