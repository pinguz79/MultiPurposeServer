namespace Finance.Desktop.Models
{
    public sealed record Conto(
        Guid Id,
        string Name,
        string DisplayName,
        decimal Balance,
        DateOnly? FirstNegativeBalanceDate = null,
        decimal? FirstNegativeBalance = null,
        CycleIndicators? CycleIndicators = null,
        RevolvingIndicators? RevolvingIndicators = null,
        bool AbilitaPedaggi = false,
        bool HasBillingCycle = false)
    {
        public bool HasCycles => CycleIndicators is not null || RevolvingIndicators is not null;
        public bool UsesCycleTimeline => HasBillingCycle || HasCycles;
    }
}
