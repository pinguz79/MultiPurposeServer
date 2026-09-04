namespace Finance.Api.Application
{
    public sealed record CycleIndicators(
        DateOnly From,
        DateOnly To,
        decimal CurrentCycleSpent,
        decimal Plafond,
        decimal OverdraftPercentage,
        decimal Overdraft,
        decimal RemainingPlafond,
        decimal RemainingIncludingOverdraft,
        decimal? PendingDebit,
        CycleThreshold? FirstPlafondExceeded,
        CycleThreshold? FirstTotalLimitExceeded);
}
