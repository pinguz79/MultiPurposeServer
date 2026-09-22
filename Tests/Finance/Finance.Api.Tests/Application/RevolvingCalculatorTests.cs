using Finance.Api.Application;

using FluentAssertions;

namespace Finance.Api.Tests.Application
{
    public class RevolvingCalculatorTests
    {
        #region CalculateInterest

        [Fact]
        public void CalculateInterest_WhenReplayingTwoAmexCycles_MatchesBothStatements()
        {
            // Arrange
            // Estratti 06/08 e 06/09/2025: date operazione, senza descrizioni o dati personali.
            var firstChanges = new Dictionary<DateOnly, decimal>
            {
                [new(2025, 7, 14)] = 3.83m,
                [new(2025, 7, 17)] = 3.98m,
                [new(2025, 7, 18)] = 0.60m,
                [new(2025, 7, 21)] = 10.27m - 152.47m,
                [new(2025, 7, 23)] = 22m + 10m + 30.06m,
                [new(2025, 7, 24)] = 13.14m,
                [new(2025, 7, 26)] = 51.10m + 7.16m + 6.59m,
                [new(2025, 7, 30)] = 10.26m,
                [new(2025, 8, 1)] = 25.69m,
                [new(2025, 8, 2)] = 2.70m,
            };
            var secondChanges = new Dictionary<DateOnly, decimal>
            {
                [new(2025, 8, 19)] = -201.97m,
                [new(2025, 8, 21)] = 60.01m,
                [new(2025, 8, 22)] = 5m,
                [new(2025, 8, 24)] = 3.96m,
                [new(2025, 8, 27)] = 3m + 15m,
                [new(2025, 8, 28)] = 11.40m,
                [new(2025, 8, 30)] = 3.60m,
                [new(2025, 8, 31)] = 7.99m,
                [new(2025, 9, 3)] = 3.39m,
                [new(2025, 9, 4)] = 11m + 13.49m,
                [new(2025, 9, 5)] = 8.28m + 1.99m,
            };
            IReadOnlyList<RevolvingCapitalDay> first = BuildDays(new(2025, 7, 7), new(2025, 8, 6), 1591.59m, firstChanges);
            IReadOnlyList<RevolvingCapitalDay> second = BuildDays(new(2025, 8, 7), new(2025, 9, 6), first[^1].Capital, secondChanges);

            // Act
            decimal firstInterest = RevolvingCalculator.CalculateInterest(first);
            decimal secondInterest = RevolvingCalculator.CalculateInterest(second);

            // Assert
            firstInterest.Should().Be(16.19m);
            secondInterest.Should().Be(15.96m);
            first[^1].Capital.Should().Be(1636.50m);
            second[^1].Capital.Should().Be(1582.64m);
        }

        [Theory]
        [InlineData(2024, 1.00)]
        [InlineData(2025, 1.00)]
        public void CalculateInterest_WhenCalendarYearVaries_UsesCalendarYearLength(int year, decimal expected)
        {
            // Arrange
            decimal capital = DateTime.IsLeapYear(year) ? 3660m : 3650m;
            RevolvingCapitalDay[] days = [new(new(year, 6, 1), capital, 0.10m)];

            // Act
            decimal interest = RevolvingCalculator.CalculateInterest(days);

            // Assert
            interest.Should().Be(expected);
        }

        [Fact]
        public void CalculateInterest_WhenDatesAreDuplicated_ThrowsArgumentException()
        {
            // Arrange
            RevolvingCapitalDay[] days = [new(new(2026, 9, 1), 100m, 0.12m), new(new(2026, 9, 1), 100m, 0.12m)];

            // Act
            var action = () => RevolvingCalculator.CalculateInterest(days);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        #endregion

        #region CalculatePayment

        [Theory]
        [InlineData(5650, 500)]
        [InlineData(500, 500)]
        [InlineData(120, 120)]
        [InlineData(0, 0)]
        [InlineData(-10, 0)]
        public void CalculateFixedPayment_WhenDebtVaries_CapsPaymentWithoutAddingOverdraft(decimal balance, decimal expected)
        {
            // Arrange

            // Act
            decimal payment = RevolvingCalculator.CalculateFixedPayment(balance, 500m);

            // Assert
            payment.Should().Be(expected);
        }

        [Fact]
        public void AllocatePayment_WhenPaymentIsNegativeZero_ReturnsZeroAllocation()
        {
            // Arrange
            var negativeZero = new decimal(0, 0, 0, true, 2);

            // Act
            RevolvingPaymentAllocation result = RevolvingCalculator.AllocatePayment(negativeZero, 0m);

            // Assert
            result.Charges.Should().Be(0m);
            result.Capital.Should().Be(0m);
        }

        [Theory]
        [InlineData(1700.95, 271.05)]
        [InlineData(1654.69, 220.16)]
        [InlineData(1247.25, 124.73)]
        [InlineData(50, 50)]
        [InlineData(100, 72.32)]
        [InlineData(0, 0)]
        [InlineData(-10, 0)]
        public void CalculatePayment_WhenDebtVaries_RespectsMinimumOverdraftAndDebtLimit(decimal balance, decimal expected)
        {
            // Arrange
            const decimal limit = 1600m;

            // Act
            decimal payment = RevolvingCalculator.CalculatePayment(balance, limit, 0.10m, 72.32m);

            // Assert
            payment.Should().Be(expected);
        }

        [Theory]
        [InlineData(169.65, 17.18, 152.47)]
        [InlineData(220.16, 18.19, 201.97)]
        public void AllocatePayment_WhenPreviousChargesArePaid_MatchesAmexCapitalAllocation(decimal payment, decimal charges, decimal capital)
        {
            // Arrange

            // Act
            RevolvingPaymentAllocation allocation = RevolvingCalculator.AllocatePayment(payment, charges);

            // Assert
            allocation.Charges.Should().Be(charges);
            allocation.Capital.Should().Be(capital);
            (allocation.Charges + allocation.Capital).Should().Be(payment);
        }

        #endregion

        [Theory]
        [InlineData(69.99, 70, 0)]
        [InlineData(70, 70, 2)]
        [InlineData(1500, 70, 2)]
        [InlineData(0, 70, 0)]
        [InlineData(0, 0, 2)]
        [InlineData(-10, 0, 2)]
        public void CalculateStampDuty_WhenThresholdVaries_AppliesInclusiveThresholdAndZeroOverride(decimal balance, decimal threshold, decimal expected)
        {
            // Arrange

            // Act
            decimal duty = RevolvingCalculator.CalculateStampDuty(balance, 2m, threshold);

            // Assert
            duty.Should().Be(expected);
        }

        private static IReadOnlyList<RevolvingCapitalDay> BuildDays(DateOnly from, DateOnly to, decimal capital, IReadOnlyDictionary<DateOnly, decimal> changes)
        {
            var days = new List<RevolvingCapitalDay>();
            for (int day = from.DayNumber; day <= to.DayNumber; day++)
            {
                DateOnly date = DateOnly.FromDayNumber(day);
                capital += changes.GetValueOrDefault(date);
                days.Add(new RevolvingCapitalDay(date, capital, 0.12m));
            }

            return days;
        }
    }
}
