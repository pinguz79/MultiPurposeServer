namespace Finance.Api.Application
{
    public sealed record ContoStatus(
        decimal Balance,
        DateOnly? FirstNegativeBalanceDate,
        decimal? FirstNegativeBalance,
        CycleIndicators? CycleIndicators = null);
}
