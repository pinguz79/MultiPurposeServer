namespace Finance.Client
{
    /// <summary>Saldo e indicatori di un conto ricevuti dalle API Finance.</summary>
    public sealed record Conto(Guid Id, string Name, string DisplayName, decimal Balance, DateOnly? FirstNegativeBalanceDate = null, decimal? FirstNegativeBalance = null,
        CycleIndicators? CycleIndicators = null, RevolvingIndicators? RevolvingIndicators = null, bool AbilitaPedaggi = false, bool HasBillingCycle = false, decimal? PreviousPeriodBalance = null);
}
