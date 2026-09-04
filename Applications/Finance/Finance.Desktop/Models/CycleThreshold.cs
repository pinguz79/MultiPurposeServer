namespace Finance.Desktop.Models
{
    public sealed record CycleThreshold(DateOnly Date, decimal Balance, decimal Excess);
}
