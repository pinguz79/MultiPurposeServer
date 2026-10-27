using FluentAssertions;

using Skating.Core;

namespace Skating.Tests
{
    public sealed class ManifestationTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WhenNameIsMissing_ThrowsArgumentException(string name)
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var action = () => new Manifestation(id, name);

            // Assert
            action.Should().Throw<ArgumentException>().WithParameterName("name");
        }

        [Fact]
        public void Constructor_WhenIdIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var action = () => new Manifestation(id, "Manifestazione demo");

            // Assert
            action.Should().Throw<ArgumentException>().WithParameterName("id");
        }

        [Fact]
        public void Constructor_WhenNameHasOuterSpaces_TrimsNameAndPreservesId()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var manifestation = new Manifestation(id, "  Manifestazione demo  ");

            // Assert
            manifestation.Id.Should().Be(id);
            manifestation.Name.Should().Be("Manifestazione demo");
        }
    }
}
