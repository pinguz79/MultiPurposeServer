using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using FluentAssertions;

namespace Finance.Api.Tests.Application
{
    public class ContoBillingCycleTests
    {
        [Theory]
        [InlineData(27, true)]
        [InlineData(31, true)]
        [InlineData(0, false)]
        public void TimelineAccountExposesCycleWithoutIndicators(int day, bool expected)
        {
            var conto = new Conto { Parametri = [new ParametroConto { Name = "ChiusuraCiclo", Type = TipoParametroConto.Intero, Value = day }] };
            new ContoDto(conto, 0).HasBillingCycle.Should().Be(expected);
        }

        [Fact]
        public void ExpiredCycleDoesNotEnableTimeline()
        {
            var conto = new Conto { Parametri = [new ParametroConto { Name = "ChiusuraCiclo", Type = TipoParametroConto.Intero, Value = 27,
                ValidTo = DateOnly.FromDateTime(DateTime.Today).AddDays(-1) }] };
            new ContoDto(conto, 0).HasBillingCycle.Should().BeFalse();
        }
    }
}
