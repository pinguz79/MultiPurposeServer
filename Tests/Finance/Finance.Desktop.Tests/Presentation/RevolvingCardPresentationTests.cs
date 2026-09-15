using Finance.Desktop.Models;
using Finance.Desktop.Presentation;

using FluentAssertions;

namespace Finance.Desktop.Tests.Presentation
{
    public class RevolvingCardPresentationTests
    {
        [Theory]
        [InlineData(100, 260, "normal")]
        [InlineData(0, 160, "normal")]
        [InlineData(-1, 159, "orange")]
        [InlineData(-160, 0, "orange")]
        [InlineData(-161, -1, "red")]
        public void GetBalanceColor_WhenNearThresholds_UsesStrictExcess(decimal remaining, decimal includingOverdraft, string expected)
        {
            // Arrange
            var indicators = new RevolvingIndicators(1600m, 160m, remaining, includingOverdraft);

            // Act
            Color result = RevolvingCardPresentation.GetBalanceColor(indicators);

            // Assert
            result.Should().Be(expected == "red" ? Color.Firebrick : expected == "orange" ? Color.DarkOrange : SystemColors.ControlText);
        }
    }
}
