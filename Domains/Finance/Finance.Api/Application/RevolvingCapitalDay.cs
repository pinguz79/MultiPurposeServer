namespace Finance.Api.Application
{
    public sealed record RevolvingCapitalDay(DateOnly Date, decimal Capital, decimal AnnualRate);
}
