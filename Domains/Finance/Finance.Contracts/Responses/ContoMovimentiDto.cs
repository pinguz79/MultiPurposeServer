namespace Finance.Contracts.Responses
{
    public sealed record ContoMovimentiDto(
        ContoDto Conto,
        int SelectedMonth,
        int SelectedYear,
        DateOnly From,
        DateOnly To,
        decimal OpeningBalance,
        decimal ClosingBalance,
        IReadOnlyList<MovimentoDto> Items);
}
