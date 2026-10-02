namespace Finance.Contracts.Responses
{
    public sealed record MovimentoDto(Guid Id, DateOnly Date, string Description, decimal Amount, decimal BalanceAfter, bool IsConfirmed = false, Guid? GruppoMovimentiId = null);
}
