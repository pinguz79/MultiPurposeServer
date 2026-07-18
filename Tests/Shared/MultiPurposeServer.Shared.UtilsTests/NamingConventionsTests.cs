using FluentAssertions;
using MultiPurposeServer.Shared.Utils;

namespace MultiPurposeServer.Shared.UtilsTests
{
    public class NamingConventionsTests
    {
        [Fact]
        public void Original_WhenNameIsProvided_ReturnsOriginalName()
        {
            // Arrange
            const string fileName = "Shooting_Marco_001.jpg";

            // Act
            var result = new NamingConventions(fileName);

            // Assert
            result.Original.Should().Be(fileName);
        }

        [Theory]
        [InlineData("Shooting_Marco_001.jpg", "Shooting_Marco_001")]
        [InlineData("Shooting_Marco_001", "Shooting_Marco_001")]
        [InlineData(@"C:\Photos\Shooting_Marco_001.jpg", "Shooting_Marco_001")]
        public void NameWithoutExtension_WhenNameIsProvided_ReturnsFileNameWithoutExtension(string fileName, string expected)
        {
            // Arrange

            // Act
            var result = new NamingConventions(fileName);

            // Assert
            result.NameWithoutExtension.Should().Be(expected);
        }

        [Theory]
        [InlineData("Shooting_Marco_001.jpg", new[] { "Shooting", "Marco", "001" })]
        [InlineData("Shooting__Marco__001.jpg", new[] { "Shooting", "Marco", "001" })]
        [InlineData(" Shooting _ Marco _ 001 .jpg", new[] { "Shooting", "Marco", "001" })]
        [InlineData("Shooting.jpg", new[] { "Shooting" })]
        public void Tokens_WhenNameContainsUnderscores_ReturnsTrimmedNonEmptyTokens(string fileName, string[] expected)
        {
            // Arrange

            // Act
            var result = new NamingConventions(fileName);

            // Assert
            result.Tokens.Should().Equal(expected);
        }

        [Theory]
        [InlineData("Shooting_Marco_001.jpg", "001")]
        [InlineData("Shooting_Marco_001bn.jpg", "001bn")]
        [InlineData("Shooting.jpg", "Shooting")]
        [InlineData("Shooting_Marco_.jpg", "Marco")]
        public void Suffix_WhenNameIsProvided_ReturnsLastToken(string fileName, string expected)
        {
            // Arrange

            // Act
            var result = new NamingConventions(fileName);

            // Assert
            result.Suffix.Should().Be(expected);
        }

        [Theory]
        [InlineData("Shooting_Marco_001.jpg", "001")]
        [InlineData("Shooting_Marco_001bn.jpg", "001bn")]
        [InlineData("Shooting_Marco_12A.jpg", "12A")]
        [InlineData("Shooting_Marco_9.jpg", "9")]
        public void SelectionCode_WhenSuffixStartsWithDigit_ReturnsSuffix(string fileName, string expected)
        {
            // Arrange

            // Act
            var result = new NamingConventions(fileName);

            // Assert
            result.SelectionCode.Should().Be(expected);
        }

        [Theory]
        [InlineData("Shooting_Marco_final.jpg")]
        [InlineData("Shooting.jpg")]
        [InlineData("Shooting_Marco_A001.jpg")]
        [InlineData("Shooting_Marco_.jpg")]
        public void SelectionCode_WhenSuffixDoesNotStartWithDigit_ReturnsNull(string fileName)
        {
            // Arrange

            // Act
            var result = new NamingConventions(fileName);

            // Assert
            result.SelectionCode.Should().BeNull();
        }

        [Fact]
        public void Tokens_WhenNameContainsPath_UsesOnlyFileName()
        {
            // Arrange
            const string fileName = @"Portfolio\Modelle\Anna\Shooting_Anna_003.jpg";

            // Act
            var result = new NamingConventions(fileName);

            // Assert
            result.Tokens.Should().Equal("Shooting", "Anna", "003");
        }

        [Fact]
        public void SelectionCode_WhenExtensionIsUppercase_ReturnsSelectionCode()
        {
            // Arrange
            const string fileName = "Shooting_Anna_003.JPG";

            // Act
            var result = new NamingConventions(fileName);

            // Assert
            result.SelectionCode.Should().Be("003");
        }
    }
}