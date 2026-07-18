using FluentAssertions;
using MultiPurposeServer.Shared.Utils;

namespace MultiPurposeServer.Shared.UtilsTests
{
    public class FileNameFormatOptionsTests
    {
        [Fact]
        public void Default_WhenRequestedMultipleTimes_ReturnsDifferentInstances()
        {
            // Arrange

            // Act
            var first = FileNameFormatOptions.Default;
            var second = FileNameFormatOptions.Default;

            // Assert
            first.Should().NotBeSameAs(second);
        }

        [Fact]
        public void Default_WhenRequested_ReturnsExpectedDefaultValues()
        {
            // Arrange

            // Act
            var options = FileNameFormatOptions.Default;

            // Assert
            options.RemoveExtension.Should().BeTrue();
            options.RemoveLeadingNumericIndex.Should().BeTrue();
            options.RemoveTrailingNumericIndex.Should().BeTrue();
            options.RemoveStandaloneSmallNumbers.Should().BeTrue();
            options.SplitCamelCase.Should().BeTrue();
            options.SplitLetterDigitBoundaries.Should().BeTrue();
            options.NormalizeUnderscores.Should().BeTrue();
            options.NormalizeHyphens.Should().BeTrue();
            options.ApplyTitleCase.Should().BeTrue();
            options.EmptyFallback.Should().Be("File");
            options.TokenMap.Should().NotBeNull();
        }

        [Fact]
        public void Default_WhenModified_DoesNotAffectSubsequentInstances()
        {
            // Arrange
            var first = FileNameFormatOptions.Default;

            // Act
            first.RemoveExtension = false;
            first.ApplyTitleCase = false;
            first.TokenMap["abc"] = "xyz";

            var second = FileNameFormatOptions.Default;

            // Assert
            second.RemoveExtension.Should().BeTrue();
            second.ApplyTitleCase.Should().BeTrue();
            second.TokenMap.Should().NotContainKey("abc");
        }

        [Fact]
        public void TokenMap_WhenDefaultIsCreated_IsCaseInsensitive()
        {
            // Arrange

            // Act
            var options = FileNameFormatOptions.Default;

            // Assert
            options.TokenMap.Comparer.Should().Be(StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void TokenMap_WhenAddingLowercaseKey_CanBeRetrievedUsingUppercaseKey()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;

            // Act
            options.TokenMap["fashion"] = "Moda";

            // Assert
            options.TokenMap["FASHION"].Should().Be("Moda");
        }

        [Fact]
        public void TokenMap_WhenAddingUppercaseKey_CanBeRetrievedUsingLowercaseKey()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;

            // Act
            options.TokenMap["FASHION"] = "Moda";

            // Assert
            options.TokenMap["fashion"].Should().Be("Moda");
        }

        [Fact]
        public void TokenMap_WhenTwoKeysDifferOnlyByCase_ContainsSingleEntry()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;

            // Act
            options.TokenMap["fashion"] = "Moda";
            options.TokenMap["FASHION"] = "Fashion";

            // Assert
            options.TokenMap.Should().HaveCount(1);
            options.TokenMap["fashion"].Should().Be("Fashion");
        }
    }
}