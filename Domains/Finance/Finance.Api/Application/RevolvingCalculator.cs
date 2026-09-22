namespace Finance.Api.Application
{
    public static class RevolvingCalculator
    {
        public static decimal CalculateInterest(IEnumerable<RevolvingCapitalDay> days)
        {
            decimal interest = 0m;
            DateOnly? previous = null;
            foreach (RevolvingCapitalDay day in days)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(day.AnnualRate, 0m);
                if (previous is not null && day.Date.DayNumber != previous.Value.DayNumber + 1)
                {
                    throw new ArgumentException("Il capitale giornaliero deve coprire giorni consecutivi senza duplicati.", nameof(days));
                }

                // Non arrotondare i singoli giorni: gli oneri sono già esclusi dalla base capitale.
                interest += Math.Max(0m, day.Capital) * day.AnnualRate / (DateTime.IsLeapYear(day.Date.Year) ? 366 : 365);
                previous = day.Date;
            }

            return decimal.Round(interest, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal CalculateStampDuty(decimal balanceIncludingInterest, decimal amount, decimal threshold)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(amount, 0m);
            ArgumentOutOfRangeException.ThrowIfLessThan(threshold, 0m);
            return threshold == 0m || balanceIncludingInterest >= threshold ? amount : 0m;
        }

        public static decimal CalculatePayment(decimal closingBalance, decimal creditLimit, decimal installmentRate, decimal minimumPayment)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(creditLimit, 0m);
            ArgumentOutOfRangeException.ThrowIfLessThan(installmentRate, 0m);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(installmentRate, 1m);
            ArgumentOutOfRangeException.ThrowIfLessThan(minimumPayment, 0m);
            if (closingBalance <= 0m)
            {
                return 0m;
            }

            decimal installment = Math.Max(decimal.Round(closingBalance * installmentRate, 2, MidpointRounding.AwayFromZero), minimumPayment);
            return Math.Min(closingBalance, installment + Math.Max(0m, closingBalance - creditLimit));
        }

        public static decimal CalculateFixedPayment(decimal closingBalance, decimal installment)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(installment, 0m);
            return Math.Min(Math.Max(0m, closingBalance), installment);
        }

        public static RevolvingPaymentAllocation AllocatePayment(decimal payment, decimal outstandingCharges)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(payment, 0m);
            ArgumentOutOfRangeException.ThrowIfLessThan(outstandingCharges, 0m);
            decimal charges = Math.Min(payment, outstandingCharges);
            return new RevolvingPaymentAllocation(charges, payment - charges);
        }
    }
}
