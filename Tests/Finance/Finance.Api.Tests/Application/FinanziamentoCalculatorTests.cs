using Finance.Api.Application;
using Finance.DataModel.Models;

using FluentAssertions;

namespace Finance.Api.Tests.Application
{
    public class FinanziamentoCalculatorTests
    {
        [Fact]
        public void Calculate_Findomestic_UsesVerifiedPrincipalAndContractualTotal()
        {
            // Arrange
            Finanziamento loan = CreateLoan();
            loan.Riallineamenti.Add(new RiallineamentoFinanziamento { InstallmentNumber = 7, Principal = 19422.49m });

            // Act
            PianoFinanziamento plan = FinanziamentoCalculator.Calculate(loan, new DateOnly(2026, 10, 1));

            // Assert
            plan.RemainingPrincipal.Should().Be(19422.49m);
            plan.RemainingInstallments.Should().Be(113);
            plan.RemainingTotal.Should().Be(37233.50m);
            plan.LastDueDate.Should().Be(new DateOnly(2036, 2, 20));
            plan.LastAlignment!.Number.Should().Be(7);
            plan.Installments[7].Interest.Should().Be(decimal.Round(19422.49m * .1345m / 12m, 2, MidpointRounding.AwayFromZero));
        }

        [Theory]
        [InlineData(19, 120)]
        [InlineData(20, 120)]
        [InlineData(21, 119)]
        public void Calculate_UsesStrictPastBoundary(int day, int expected)
        {
            // Act
            PianoFinanziamento plan = FinanziamentoCalculator.Calculate(CreateLoan(), new DateOnly(2026, 3, day));

            // Assert
            plan.RemainingInstallments.Should().Be(expected);
        }

        [Theory]
        [InlineData(2024, 29)]
        [InlineData(2025, 28)]
        public void Calculate_ClampsShortMonthsWithoutLosingContractualDay(int year, int day)
        {
            // Arrange
            Finanziamento loan = CreateLoan();
            loan.FirstDueDate = new DateOnly(year, 1, 31);
            loan.DueDay = 31;

            // Assert
            FinanziamentoCalculator.GetDueDate(loan, 2).Should().Be(new DateOnly(year, 2, day));
            FinanziamentoCalculator.GetDueDate(loan, 3).Should().Be(new DateOnly(year, 3, 31));
        }

        [Fact]
        public void Calculate_AlignmentDoesNotRewritePreviousRows()
        {
            // Arrange
            Finanziamento loan = CreateLoan();
            DateOnly today = new(2027, 1, 1);
            PianoFinanziamento original = FinanziamentoCalculator.Calculate(loan, today);
            loan.Riallineamenti.Add(new RiallineamentoFinanziamento { InstallmentNumber = 7, Principal = 19000m });
            loan.Riallineamenti.Add(new RiallineamentoFinanziamento { InstallmentNumber = 9, Principal = 18000m });

            // Act
            PianoFinanziamento plan = FinanziamentoCalculator.Calculate(loan, today);

            // Assert
            plan.Installments.Take(6).Should().Equal(original.Installments.Take(6));
            plan.Installments[6].Principal.Should().Be(original.Installments[6].Principal);
            plan.LastAlignment!.Number.Should().Be(9);
            plan.Installments[9].Interest.Should().Be(decimal.Round(18000m * loan.Tan / 12m, 2, MidpointRounding.AwayFromZero));
        }

        [Fact]
        public void Calculate_ZeroPrincipalRetainsInstallmentsAndReportsDiscrepancy()
        {
            // Arrange
            Finanziamento loan = CreateLoan();
            loan.InitialPrincipal = 100m;
            loan.Tan = 0;
            loan.Installment = 60m;
            loan.InstallmentCount = 3;

            // Act
            PianoFinanziamento plan = FinanziamentoCalculator.Calculate(loan, new DateOnly(2026, 3, 1));

            // Assert
            plan.Installments.Select(row => row.Principal).Should().Equal(60m, 40m, 0m);
            plan.Installments.Should().OnlyContain(row => row.RemainingPrincipal >= 0 && row.Interest == 0);
            plan.RemainingTotal.Should().Be(256.50m);
            plan.TotalDiscrepancy.Should().Be(80m);
            loan.IsClosed.Should().BeFalse();
        }

        [Fact]
        public void Calculate_RoundsMidpointAwayFromZeroAndReportsUnpaidFinalPrincipal()
        {
            // Arrange
            Finanziamento loan = CreateLoan();
            loan.InitialPrincipal = 100.50m;
            loan.Tan = .12m;
            loan.Installment = 10m;
            loan.InstallmentCount = 1;

            // Act
            PianoFinanziamento plan = FinanziamentoCalculator.Calculate(loan, new DateOnly(2026, 4, 1));

            // Assert
            plan.Installments[0].Interest.Should().Be(1.01m);
            plan.FinalPrincipal.Should().Be(91.51m);
            plan.RemainingInstallments.Should().Be(0);
            plan.RemainingPrincipal.Should().Be(91.51m);
        }

        private static Finanziamento CreateLoan() => new()
        {
            InitialPrincipal = 20000m, Tan = .1345m, Installment = 304m, Insurance = 25.50m,
            FirstDueDate = new DateOnly(2026, 3, 20), DueDay = 20, InstallmentCount = 120,
        };
    }
}
