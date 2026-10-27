using FluentAssertions;

using Skating.Core;

namespace Skating.Tests
{
    public sealed class ScoreValueTests
    {
        [Theory]
        [InlineData("", null)]
        [InlineData("0", 0)]
        [InlineData("10", 100)]
        [InlineData("7,8", 78)]
        [InlineData("7.8", 78)]
        public void Parse_WhenValid_ReturnsTenthsOrMissing(string text, int? expected)
        {
            // Arrange

            // Act
            var result = ScoreValue.Parse(text);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("11")]
        [InlineData("-1")]
        [InlineData("7.88")]
        [InlineData("1,000")]
        [InlineData("x")]
        public void Parse_WhenInvalid_RejectsWithoutRounding(string text)
        {
            // Arrange

            // Act
            var action = () => ScoreValue.Parse(text);

            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}
