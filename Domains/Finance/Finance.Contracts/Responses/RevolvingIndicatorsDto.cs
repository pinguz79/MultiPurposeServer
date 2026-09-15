namespace Finance.Contracts.Responses
{
    public class RevolvingIndicatorsDto(decimal plafond, decimal overdraft, decimal remainingPlafond, decimal remainingIncludingOverdraft)
    {
        public decimal Plafond { get; set; } = plafond;
        public decimal Overdraft { get; set; } = overdraft;
        public decimal RemainingPlafond { get; set; } = remainingPlafond;
        public decimal RemainingIncludingOverdraft { get; set; } = remainingIncludingOverdraft;
    }
}
