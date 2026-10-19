namespace Finance.Client
{
    /// <summary>Speso, disponibilità e soglie previste del ciclo di una carta a saldo.</summary>
    public sealed record CycleIndicators(DateOnly From, DateOnly To, decimal CurrentCycleSpent, decimal Plafond, decimal OverdraftPercentage, decimal Overdraft, decimal RemainingPlafond,
        decimal RemainingIncludingOverdraft, decimal? PendingDebit, CycleThreshold? FirstPlafondExceeded, CycleThreshold? FirstTotalLimitExceeded);
}
