namespace Finance.Contracts.Responses
{
    public sealed record CycleIndicatorsDto(
        DateOnly From,
        DateOnly To,
        decimal CurrentCycleSpent,
        decimal Plafond,
        decimal OverdraftPercentage,
        decimal Overdraft,
        decimal RemainingPlafond,
        decimal RemainingIncludingOverdraft,
        decimal? PendingDebit,
        CycleThresholdDto? FirstPlafondExceeded,
        CycleThresholdDto? FirstTotalLimitExceeded);
}
