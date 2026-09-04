namespace Finance.Contracts.Responses
{
    public sealed record ContoCicliDto(
        ContoDto Conto,
        int SelectedMonth,
        int SelectedYear,
        DateOnly From,
        DateOnly To,
        decimal OpeningBalance,
        decimal ClosingBalance,
        IReadOnlyList<CicloDto> Cycles);
}
