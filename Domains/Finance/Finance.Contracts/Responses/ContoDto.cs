using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class ContoDto(
        Conto conto,
        decimal balance,
        DateOnly? firstNegativeBalanceDate = null,
        decimal? firstNegativeBalance = null,
        CycleIndicatorsDto? cycleIndicators = null,
        RevolvingIndicatorsDto? revolvingIndicators = null)
    {
        public Guid Id { get; set; } = conto.Id;
        public string Name { get; set; } = conto.Name;
        public string DisplayName { get; set; } = conto.DisplayName;
        public bool AbilitaPedaggi { get; set; } = conto.Parametri.OrderBy(parametro => parametro.Index).FirstOrDefault(parametro =>
            string.Equals(parametro.Name, "AbilitaPedaggi", StringComparison.OrdinalIgnoreCase)
            && (parametro.ValidFrom is null || parametro.ValidFrom <= DateOnly.FromDateTime(DateTime.Today))
            && (parametro.ValidTo is null || parametro.ValidTo >= DateOnly.FromDateTime(DateTime.Today))) is { Type: TipoParametroConto.Booleano, Value: 1m };
        public decimal Balance { get; set; } = balance;
        public DateOnly? FirstNegativeBalanceDate { get; set; } = firstNegativeBalanceDate;
        public decimal? FirstNegativeBalance { get; set; } = firstNegativeBalance;
        public CycleIndicatorsDto? CycleIndicators { get; set; } = cycleIndicators;
        public RevolvingIndicatorsDto? RevolvingIndicators { get; set; } = revolvingIndicators;
    }
}
