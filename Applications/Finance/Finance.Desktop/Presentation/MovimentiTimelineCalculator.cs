using Finance.Desktop.Models;

namespace Finance.Desktop.Presentation
{
    internal static class MovimentiTimelineCalculator
    {
        public static MonthSummary CalculateMonth(ContoMovimenti timeline, DateOnly month, DateOnly today)
        {
            Movimento[] movements = [.. timeline.Items.Where(movement => movement.Date.Year == month.Year && movement.Date.Month == month.Month)];
            decimal? currentBalance = month.Year == today.Year && month.Month == today.Month && today >= timeline.From && today <= timeline.To
                ? timeline.Items.LastOrDefault(movement => movement.Date <= today)?.BalanceAfter ?? timeline.OpeningBalance
                : null;
            return new(movements, movements.Sum(movement => movement.Amount), currentBalance);
        }
    }
}
