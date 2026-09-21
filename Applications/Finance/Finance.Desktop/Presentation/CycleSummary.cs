using Finance.Desktop.Models;

namespace Finance.Desktop.Presentation
{
    public sealed class CycleSummary(Ciclo cycle, decimal openingBalance, DateOnly today, ParametroConto? plafond)
    {
        public decimal OpeningBalance { get; } = openingBalance;
        public decimal ClosingBalance { get; } = openingBalance + cycle.Items.Sum(item => item.Amount);
        public bool IsOpen { get; } = cycle.From <= today && today <= cycle.To;
        public bool IsFuture { get; } = cycle.From > today;
        public DateOnly ReferenceDate { get; } = cycle.From <= today && today <= cycle.To ? today : cycle.To;
        public decimal ReferenceBalance { get; } = openingBalance + cycle.Items.Where(item => item.Date <= (cycle.From <= today && today <= cycle.To ? today : cycle.To)).Sum(item => item.Amount);

        public decimal? RemainingPlafond => plafond?.Definitions.OrderBy(definition => definition.Index).FirstOrDefault(definition => (definition.ValidFrom is null || definition.ValidFrom <= ReferenceDate) && (definition.ValidTo is null || definition.ValidTo >= ReferenceDate))?.Value - ReferenceBalance;

        public decimal? StatementRemainingPlafond => RemainingPlafond is decimal remaining ? decimal.Floor(Math.Max(0m, remaining)) : null;
    }
}
