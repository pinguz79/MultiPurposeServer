using FluentAssertions;

using Skating.Core;

namespace Skating.Tests
{
    public sealed class RegistryModelTests
    {
        [Fact]
        public void Athlete_WhenNamesHaveOuterSpaces_TrimsNamesAndPreservesIdentity()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var athlete = new Athlete(id, "  Marco  ", "  Lepri  ");

            // Assert
            athlete.Id.Should().Be(id);
            athlete.FirstName.Should().Be("Marco");
            athlete.LastName.Should().Be("Lepri");
        }

        [Theory]
        [InlineData("", "Lepri", "firstName")]
        [InlineData("Marco", "  ", "lastName")]
        public void Athlete_WhenRequiredNameIsMissing_ThrowsArgumentException(string firstName, string lastName, string parameter)
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var action = () => new Athlete(id, firstName, lastName);

            // Assert
            action.Should().Throw<ArgumentException>().WithParameterName(parameter);
        }

        [Theory]
        [InlineData("PRO", false)]
        [InlineData("Avanzato", true)]
        public void Level_WhenNameDiffersFromAwardPath_PreservesExplicitProFlag(string name, bool isPro)
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var level = new Level(id, name, isPro);

            // Assert
            level.IsPro.Should().Be(isPro);
        }

        [Theory]
        [InlineData("athlete")]
        [InlineData("discipline")]
        [InlineData("category")]
        [InlineData("level")]
        public void RegistryEntity_WhenIdIsEmpty_ThrowsArgumentException(string kind)
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var action = () => CreateEntity(kind, id, "Demo");

            // Assert
            action.Should().Throw<ArgumentException>().WithParameterName("id");
        }

        [Theory]
        [InlineData("discipline")]
        [InlineData("category")]
        [InlineData("level")]
        public void RegistryEntity_WhenNameIsBlank_ThrowsArgumentException(string kind)
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var action = () => CreateEntity(kind, id, "   ");

            // Assert
            action.Should().Throw<ArgumentException>().WithParameterName("name");
        }

        private static object CreateEntity(string kind, Guid id, string name) => kind switch
        {
            "athlete" => new Athlete(id, name, "Cognome"),
            "discipline" => new Discipline(id, name),
            "category" => new Category(id, name),
            "level" => new Level(id, name, false),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
