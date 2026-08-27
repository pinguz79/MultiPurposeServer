namespace Finance.Desktop.Models
{
    public sealed record Movimento(Guid Id, DateOnly Date, string Description, decimal Amount, decimal BalanceAfter);
}
