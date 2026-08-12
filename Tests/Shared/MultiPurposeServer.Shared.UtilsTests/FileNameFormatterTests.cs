using FluentAssertions;

using MultiPurposeServer.Shared.Utils;

namespace MultiPurposeServer.Shared.UtilsTests
{
    public class FileNameFormatterTests
    {
        [Theory]
        [InlineData(null, "File")]
        [InlineData("", "File")]
        [InlineData(" ", "File")]
        [InlineData("\t\r\n", "File")]
        public void HumanizedName_WhenFileNameIsMissing_ReturnsDefaultFallback(string? fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenFileNameIsMissing_ReturnsConfiguredFallback()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;
            options.EmptyFallback = "Fotografia";
            var formatter = new FileNameFormatter(null, options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("Fotografia");
        }

        [Theory]
        [InlineData("ritratto.jpg", "Ritratto")]
        [InlineData("ritratto.jpeg", "Ritratto")]
        [InlineData("ritratto.JPG", "Ritratto")]
        [InlineData("ritratto.2026", "Ritratto")]
        public void HumanizedName_WhenRemoveExtensionIsEnabled_RemovesExtension(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenRemoveExtensionIsDisabled_PreservesExtension()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.RemoveExtension = false;
            var formatter = new FileNameFormatter("ritratto.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("ritratto.jpg");
        }

        [Theory]
        [InlineData("001_ritratto.jpg", "Ritratto")]
        [InlineData("12-ritratto.jpg", "Ritratto")]
        [InlineData("7 ritratto.jpg", "Ritratto")]
        [InlineData("  999 _ ritratto.jpg", "Ritratto")]
        public void HumanizedName_WhenLeadingNumericIndexIsPresent_RemovesIt(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenLeadingNumericIndexRemovalIsDisabled_PreservesIndex()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.RemoveLeadingNumericIndex = false;
            var formatter = new FileNameFormatter("001_ritratto.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("001_ritratto");
        }

        [Theory]
        [InlineData("ritratto_01.jpg", "Ritratto")]
        [InlineData("ritratto-123.jpg", "Ritratto")]
        [InlineData("ritratto 99999.jpg", "Ritratto")]
        public void HumanizedName_WhenTrailingNumericIndexIsPresent_RemovesIt(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("ritratto_1.jpg", "Ritratto")]
        [InlineData("ritratto_123456.jpg", "Ritratto 123456")]
        public void HumanizedName_WhenTrailingNumericIndexIsOutsideConfiguredLength_DoesNotUseTrailingIndexRule(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenTrailingNumericIndexRemovalIsDisabled_PreservesIndex()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.RemoveTrailingNumericIndex = false;
            var formatter = new FileNameFormatter("ritratto_001.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("ritratto_001");
        }

        [Theory]
        [InlineData("ritratto_1_modella.jpg", "Ritratto Modella")]
        [InlineData("ritratto 12 modella.jpg", "Ritratto Modella")]
        [InlineData("ritratto-999-modella.jpg", "Ritratto – Modella")]
        public void HumanizedName_WhenStandaloneSmallNumbersArePresent_RemovesThem(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenStandaloneSmallNumberRemovalIsDisabled_PreservesNumbers()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.RemoveStandaloneSmallNumbers = false;
            var formatter = new FileNameFormatter("ritratto_12_modella.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("ritratto_12_modella");
        }

        [Theory]
        [InlineData("ritratto_modella.jpg", "Ritratto Modella")]
        [InlineData("ritratto__modella.jpg", "Ritratto Modella")]
        [InlineData("ritratto___in___studio.jpg", "Ritratto In Studio")]
        public void HumanizedName_WhenUnderscoresArePresent_NormalizesThem(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenUnderscoreNormalizationIsDisabled_PreservesUnderscores()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.NormalizeUnderscores = false;
            var formatter = new FileNameFormatter("ritratto_modella.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("ritratto_modella");
        }

        [Theory]
        [InlineData("ritratto-modella.jpg", "Ritratto – Modella")]
        [InlineData("ritratto - modella.jpg", "Ritratto – Modella")]
        [InlineData("ritratto---modella.jpg", "Ritratto – Modella")]
        public void HumanizedName_WhenHyphensArePresent_NormalizesThem(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenHyphenNormalizationIsDisabled_PreservesHyphens()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.NormalizeHyphens = false;
            var formatter = new FileNameFormatter("ritratto-modella.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("ritratto-modella");
        }

        [Theory]
        [InlineData("ritrattoInStudio.jpg", "Ritratto In Studio")]
        [InlineData("modellaÀLaMode.jpg", "Modella Àla Mode")]
        [InlineData("perchéBella.jpg", "Perché Bella")]
        public void HumanizedName_WhenCamelCaseIsPresent_SplitsWords(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenCamelCaseSplittingIsDisabled_PreservesCamelCase()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.SplitCamelCase = false;
            var formatter = new FileNameFormatter("ritrattoInStudio.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("ritrattoInStudio");
        }

        [Theory]
        [InlineData("modella2.jpg", "Modella")]
        [InlineData("2modella.jpg", "Modella")]
        [InlineData("foto12studio.jpg", "Foto Studio")]
        [InlineData("abc123def.jpg", "Abc Def")]
        public void HumanizedName_WhenLetterDigitBoundariesArePresent_SplitsWords(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenLetterDigitBoundarySplittingIsDisabled_PreservesBoundaries()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.SplitLetterDigitBoundaries = false;
            var formatter = new FileNameFormatter("modella2.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("modella2");
        }

        [Fact]
        public void HumanizedName_WhenTokenMapContainsReplacement_ReplacesToken()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;
            options.TokenMap["bn"] = "Bianco e nero";
            var formatter = new FileNameFormatter("ritratto_bn.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("Ritratto Bianco E Nero");
        }

        [Fact]
        public void HumanizedName_WhenTokenMapKeyUsesDifferentCase_ReplacesTokenCaseInsensitively()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;
            options.TokenMap["BN"] = "Bianco e nero";
            var formatter = new FileNameFormatter("ritratto_bn.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("Ritratto Bianco E Nero");
        }

        [Fact]
        public void HumanizedName_WhenTokenMapReplacementIsNull_RemovesToken()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;
            options.TokenMap["provino"] = null;
            var formatter = new FileNameFormatter("ritratto_provino_modella.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("Ritratto Modella");
        }

        [Fact]
        public void HumanizedName_WhenTokenMapReplacementIsWhitespace_RemovesToken()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;
            options.TokenMap["provino"] = " ";
            var formatter = new FileNameFormatter("ritratto_provino_modella.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("Ritratto Modella");
        }

        [Fact]
        public void HumanizedName_WhenTokenMapIsEmpty_PreservesTokens()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;
            var formatter = new FileNameFormatter("ritratto_bn.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("Ritratto Bn");
        }

        [Theory]
        [InlineData("ritratto in studio.jpg", "Ritratto In Studio")]
        [InlineData("RITRATTO IN STUDIO.jpg", "Ritratto IN Studio")]
        [InlineData("ritratto DI MODA.jpg", "Ritratto DI Moda")]
        public void HumanizedName_WhenTitleCaseIsEnabled_AppliesItalianTitleCase(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("AI ritratto.jpg", "AI Ritratto")]
        [InlineData("VR studio.jpg", "VR Studio")]
        public void HumanizedName_WhenShortUppercaseWordsArePresent_PreservesThem(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenTitleCaseIsDisabled_PreservesOriginalCasing()
        {
            // Arrange
            var options = CreateNeutralOptions();
            options.ApplyTitleCase = false;
            var formatter = new FileNameFormatter("RITRATTO in STUDIO.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("RITRATTO in STUDIO");
        }

        [Theory]
        [InlineData(@"C:\Portfolio\Fashion\ritratto_modella_001.jpg", "Ritratto Modella")]
        [InlineData(@"Portfolio/Fashion/ritratto_modella_001.jpg", "Ritratto Modella")]
        public void HumanizedName_WhenFullPathIsProvided_UsesOnlyFileName(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("001_ritrattoModella_bn_123.jpg", "Ritratto Modella Bn")]
        [InlineData("12-shooting_inStudio-0045.JPG", "Shooting In Studio")]
        [InlineData("ritratto---modella__studio_99.jpeg", "Ritratto – Modella Studio")]
        public void HumanizedName_WhenMultipleRulesApply_ReturnsHumanizedName(string fileName, string expected)
        {
            // Arrange
            var formatter = new FileNameFormatter(fileName);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void HumanizedName_WhenAllContentIsRemoved_ReturnsConfiguredFallback()
        {
            // Arrange
            var options = FileNameFormatOptions.Default;
            options.EmptyFallback = "Fotografia";
            var formatter = new FileNameFormatter("001_123.jpg", options);

            // Act
            var result = formatter.HumanizedName;

            // Assert
            result.Should().Be("Fotografia");
        }

        [Fact]
        public void HumanizedName_WhenReadMultipleTimes_ReturnsSameValue()
        {
            // Arrange
            var formatter = new FileNameFormatter("001_ritratto_modella_123.jpg");

            // Act
            var first = formatter.HumanizedName;
            var second = formatter.HumanizedName;

            // Assert
            second.Should().Be(first);
        }

        private static FileNameFormatOptions CreateNeutralOptions()
        {
            return new FileNameFormatOptions
            {
                RemoveExtension = true,
                RemoveLeadingNumericIndex = false,
                RemoveTrailingNumericIndex = false,
                RemoveStandaloneSmallNumbers = false,
                SplitCamelCase = false,
                SplitLetterDigitBoundaries = false,
                NormalizeUnderscores = false,
                NormalizeHyphens = false,
                ApplyTitleCase = false
            };
        }
    }
}
