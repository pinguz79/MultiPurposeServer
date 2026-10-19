using System.Globalization;

using Finance.Client;

namespace Finance.Mobile
{
    public sealed class AccountCard
    {
        private const string CriticalColor = "#B3261E";
        private const string ExceededColor = "#B35A00";
        private const string NormalColor = "#15445C";

        public string Name { get; }

        public string Balance { get; }

        public string Previous { get; }

        public string Details { get; }

        public string Warning { get; }

        public string BalanceColor { get; }

        public string WarningColor { get; } = CriticalColor;

        public AccountCard(Conto conto)
        {
            Name = conto.DisplayName;
            decimal? remaining = conto.RevolvingIndicators?.RemainingPlafond ?? conto.CycleIndicators?.RemainingPlafond;
            decimal? totalRemaining = conto.RevolvingIndicators?.RemainingIncludingOverdraft ?? conto.CycleIndicators?.RemainingIncludingOverdraft;
            bool isCritical = totalRemaining < 0 || (remaining is null && conto.Balance < 0);
            BalanceColor = isCritical ? CriticalColor : remaining < 0 ? ExceededColor : NormalColor;
            Balance = conto.RevolvingIndicators is not null ? $"Debito attuale: {FormatCurrency(conto.Balance)}"
                : conto.CycleIndicators is not null ? $"Speso nel ciclo: {FormatCurrency(conto.CycleIndicators.CurrentCycleSpent)}" : FormatCurrency(conto.Balance);
            Previous = conto.PreviousPeriodBalance is { } previous ? $"(mp {FormatCurrency(previous)})" : "";
            Details = conto.RevolvingIndicators is { } revolving
                ? $"Plafond residuo: {FormatCurrency(revolving.RemainingPlafond)}\nCon scoperto: {FormatCurrency(revolving.RemainingIncludingOverdraft)}"
                : conto.CycleIndicators is { } cycle
                ? $"Plafond residuo: {FormatCurrency(cycle.RemainingPlafond)} ({FormatCurrency(cycle.RemainingIncludingOverdraft)})" +
                  (cycle.PendingDebit is { } debit ? $"\nIn addebito: {FormatCurrency(debit)}" : "") : "";
            Warning = conto.FirstNegativeBalanceDate is { } date && conto.FirstNegativeBalance is { } negative
                ? $"Prima criticità: {date:dd/MM/yyyy} {FormatCurrency(negative)}" : "";
            if (conto.RevolvingIndicators is { } amex && string.Equals(conto.Name, "AmEx", StringComparison.OrdinalIgnoreCase))
            {
                Details += $"\nDisponibile AmEx: {FormatCurrency(decimal.Floor(Math.Max(0m, amex.RemainingPlafond)))}";
            }

            if (conto.CycleIndicators?.FirstPlafondExceeded is { } exceeded)
            {
                Warning = $"Prima eccedenza prevista: {exceeded.Date:dd/MM/yyyy} {FormatCurrency(exceeded.Balance)} (+{FormatCurrency(exceeded.Excess)} oltre il limite)";
                WarningColor = ExceededColor;
            }

            if (conto.CycleIndicators?.FirstTotalLimitExceeded is { } limit)
            {
                Warning += $"\nSuperamento scoperto previsto: {limit.Date:dd/MM/yyyy} {FormatCurrency(limit.Balance)}";
                WarningColor = CriticalColor;
            }
        }

        private static string FormatCurrency(decimal amount) => amount.ToString("C2", CultureInfo.GetCultureInfo("it-IT"));
    }
}
