using Finance.Desktop.Models;

namespace Finance.Desktop.Presentation
{
    internal sealed record MonthSummary(IReadOnlyList<Movimento> Movements, decimal Delta, decimal? CurrentBalance);
}
