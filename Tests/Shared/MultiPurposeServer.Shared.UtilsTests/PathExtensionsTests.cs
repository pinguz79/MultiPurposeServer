using FluentAssertions;
using MultiPurposeServer.Shared.Utils;

namespace MultiPurposeServer.Shared.UtilsTests
{
    public class PathExtensionsTests
    {
        [Theory]
        [InlineData("Portfolio/Fashion", "Portfolio/Fashion")]
        [InlineData("Portfolio/Fashion/Milano", "Portfolio/Fashion/Milano")]
        public void NormalizedPath_WhenPathIsAlreadyNormalized_ReturnsSamePath(string path, string expected)
        {
            // Arrange

            // Act
            var result = path.NormalizedPath();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(@"Portfolio\Fashion", "Portfolio/Fashion")]
        [InlineData(@"Portfolio\Fashion\Milano", "Portfolio/Fashion/Milano")]
        [InlineData(@"\Portfolio\Fashion\", "Portfolio/Fashion")]
        public void NormalizedPath_WhenPathContainsBackslashes_ReplacesThemWithForwardSlashes(string path, string expected)
        {
            // Arrange

            // Act
            var result = path.NormalizedPath();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("/Portfolio/Fashion", "Portfolio/Fashion")]
        [InlineData("///Portfolio/Fashion", "Portfolio/Fashion")]
        public void NormalizedPath_WhenPathContainsLeadingSlashes_RemovesThem(string path, string expected)
        {
            // Arrange

            // Act
            var result = path.NormalizedPath();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Portfolio/Fashion/", "Portfolio/Fashion")]
        [InlineData("Portfolio/Fashion///", "Portfolio/Fashion")]
        public void NormalizedPath_WhenPathContainsTrailingSlashes_RemovesThem(string path, string expected)
        {
            // Arrange

            // Act
            var result = path.NormalizedPath();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(" Portfolio/Fashion ", "Portfolio/Fashion")]
        [InlineData("   /Portfolio/Fashion/   ", "Portfolio/Fashion")]
        [InlineData("\tPortfolio/Fashion\r\n", "Portfolio/Fashion")]
        public void NormalizedPath_WhenPathContainsOuterWhitespace_TrimsIt(string path, string expected)
        {
            // Arrange

            // Act
            var result = path.NormalizedPath();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Portfolio//Fashion", "Portfolio//Fashion")]
        [InlineData("Portfolio///Fashion//Milano", "Portfolio///Fashion//Milano")]
        public void NormalizedPath_WhenPathContainsRepeatedInnerSlashes_PreservesThem(string path, string expected)
        {
            // Arrange

            // Act
            var result = path.NormalizedPath();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("/")]
        [InlineData("///")]
        [InlineData(@"\")]
        [InlineData(@"\\\")]
        public void NormalizedPath_WhenPathContainsNoSegments_ReturnsEmptyString(string path)
        {
            // Arrange

            // Act
            var result = path.NormalizedPath();

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("Portfolio/Fashion", "PORTFOLIO/FASHION")]
        [InlineData("portfolio/fashion/milano", "PORTFOLIO/FASHION/MILANO")]
        [InlineData(@" Portfolio\Fashion\Milano ", "PORTFOLIO/FASHION/MILANO")]
        public void NormalizedPathForComparison_WhenPathIsProvided_ReturnsUppercaseNormalizedPath(string path, string expected)
        {
            // Arrange

            // Act
            var result = path.NormalizedPathForComparison();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Portfolio/Fashion", "portfolio/fashion")]
        [InlineData("PORTFOLIO/FASHION", "portfolio/FASHION")]
        [InlineData(@"Portfolio\Fashion\Milano", "/portfolio/fashion/milano/")]
        public void NormalizedPathForComparison_WhenPathsDifferOnlyByFormatting_ReturnsSameValue(string firstPath, string secondPath)
        {
            // Arrange

            // Act
            var firstResult = firstPath.NormalizedPathForComparison();
            var secondResult = secondPath.NormalizedPathForComparison();

            // Assert
            firstResult.Should().Be(secondResult);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("/")]
        [InlineData(@"\")]
        public void NormalizedPathForComparison_WhenPathContainsNoSegments_ReturnsEmptyString(string path)
        {
            // Arrange

            // Act
            var result = path.NormalizedPathForComparison();

            // Assert
            result.Should().BeEmpty();
        }
    }
}