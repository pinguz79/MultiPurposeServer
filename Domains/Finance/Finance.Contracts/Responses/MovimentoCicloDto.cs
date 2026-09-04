namespace Finance.Contracts.Responses
{
    public sealed record MovimentoCicloDto(
        Guid Id,
        DateOnly Date,
        string Description,
        decimal Amount,
        decimal BalanceAfter,
        decimal CycleBalanceAfter);
}
