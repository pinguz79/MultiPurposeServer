using System.Text.Json;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Configuration;

namespace Skating.Desktop.Tests
{
    public sealed class AwardOptionsTests
    {
        [Theory]
        [InlineData("Discipline", AwardAggregation.Discipline)]
        [InlineData("Category", AwardAggregation.Category)]
        public void Load_WhenNamedMode_ReturnsAggregation(string mode, AwardAggregation expected)
        {
            // Arrange
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, $$"""{"IntermediateAwardAggregation":"{{mode}}"}""");

                // Act
                var options = AwardOptions.Load(path);

                // Assert
                options.IntermediateAwardAggregation.Should().Be(expected);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Load_WhenUnknownMode_RejectsConfiguration()
        {
            // Arrange
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, """{"IntermediateAwardAggregation":"Unknown"}""");

                // Act
                var action = () => AwardOptions.Load(path);

                // Assert
                action.Should().Throw<JsonException>();
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
