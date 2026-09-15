namespace Finance.Api.Application
{
    public sealed record RevolvingIndicators(decimal Plafond, decimal Overdraft, decimal RemainingPlafond, decimal RemainingIncludingOverdraft);
}
