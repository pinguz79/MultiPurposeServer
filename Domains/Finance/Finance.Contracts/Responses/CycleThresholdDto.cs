namespace Finance.Contracts.Responses
{
    public sealed record CycleThresholdDto(DateOnly Date, decimal Balance, decimal Excess);
}
