using Finance.Desktop.Models;

using FluentAssertions;

namespace Finance.Desktop.Tests.Models
{
    public class ContoTests
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        public void HasCycles_WhenProfileProvided_RecognizesBothCardProfiles(bool settlement, bool revolving, bool expected)
        {
            // Arrange
            var conto = new Conto(Guid.NewGuid(), "Carta", "Carta", 0m,
                CycleIndicators: settlement ? new CycleIndicators(default, default, 0m, 1600m, 0.1m, 160m, 1600m, 1760m, null, null, null) : null,
                RevolvingIndicators: revolving ? new RevolvingIndicators(1600m, 160m, 1600m, 1760m) : null);

            // Act
            bool result = conto.HasCycles;

            // Assert
            result.Should().Be(expected);
        }
    }
}
