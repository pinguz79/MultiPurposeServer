using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public static class FinanziamentoCalculator
    {
        public static DateOnly GetDueDate(Finanziamento loan, int number)
        {
            DateOnly month = new DateOnly(loan.FirstDueDate.Year, loan.FirstDueDate.Month, 1).AddMonths(number - 1);
            return new DateOnly(month.Year, month.Month, Math.Min(loan.DueDay, DateTime.DaysInMonth(month.Year, month.Month)));
        }

        public static PianoFinanziamento Calculate(Finanziamento loan, DateOnly today)
        {
            Dictionary<int, decimal> alignments = loan.Riallineamenti.ToDictionary(item => item.InstallmentNumber, item => item.Principal);
            var rows = new List<RataFinanziamento>();
            decimal principal = loan.InitialPrincipal;
            decimal current = principal;
            decimal total = loan.Installment + loan.Insurance + loan.Fees;
            RataFinanziamento? lastAlignment = null;
            for (int number = 1; number <= loan.InstallmentCount; number++)
            {
                DateOnly dueDate = GetDueDate(loan, number);
                decimal interest = decimal.Round(principal * loan.Tan / 12m, 2, MidpointRounding.AwayFromZero);
                decimal capital = Math.Min(principal, loan.Installment - interest);
                decimal discrepancy = loan.Installment - interest - capital;
                principal -= capital;
                decimal? verified = alignments.TryGetValue(number, out decimal value) ? value : null;
                principal = verified ?? principal;
                var row = new RataFinanziamento(number, dueDate, dueDate < today, capital, interest, loan.Insurance, loan.Fees, total, principal, discrepancy, verified);
                rows.Add(row);
                if (row.IsPast)
                {
                    current = principal;
                    if (verified is not null)
                    {
                        lastAlignment = row;
                    }
                }
            }
            int remaining = rows.Count(row => !row.IsPast);
            return new PianoFinanziamento(today, current, remaining, remaining * total, rows[^1].DueDate, principal, rows.Sum(row => row.Discrepancy), lastAlignment, rows);
        }
    }
}
