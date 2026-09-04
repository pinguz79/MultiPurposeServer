namespace Finance.Api.Application
{
    public sealed record CycleThreshold(DateOnly Date, decimal Balance, decimal Excess);
}
