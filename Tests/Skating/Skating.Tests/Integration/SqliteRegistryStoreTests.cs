using FluentAssertions;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class SqliteRegistryStoreTests
    {
        [Fact]
        public void Registry_WhenReopened_ReturnsAllPersistedEntities()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            var athlete = new Athlete(Guid.NewGuid(), "Anna", "D'Angelo");
            var discipline = new Discipline(Guid.NewGuid(), "Assolo/Duo");
            var category = new Category(Guid.NewGuid(), "Baby");
            var level = new Level(Guid.NewGuid(), "Avanzato", true);
            store.AddAthlete(athlete);
            store.AddDiscipline(discipline);
            store.AddCategory(category);
            store.AddLevel(level);
            var reopened = new SqliteRegistryStore(archive.DatabasePath);

            // Act
            var athletes = reopened.GetAthletes();
            var disciplines = reopened.GetDisciplines();
            var categories = reopened.GetCategories();
            var levels = reopened.GetLevels();

            // Assert
            athletes.Should().ContainSingle().Which.Should().BeEquivalentTo(athlete);
            disciplines.Should().ContainSingle().Which.Should().BeEquivalentTo(discipline);
            categories.Should().ContainSingle().Which.Should().BeEquivalentTo(category);
            levels.Should().ContainSingle().Which.Should().BeEquivalentTo(level);
        }

        [Fact]
        public void AddAthlete_WhenNamesMatch_KeepsDistinctIdentities()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            var first = new Athlete(Guid.NewGuid(), "Mario", "Rossi");
            var second = new Athlete(Guid.NewGuid(), "Mario", "Rossi");
            store.AddAthlete(first);

            // Act
            store.AddAthlete(second);

            // Assert
            store.GetAthletes().Select(athlete => athlete.Id).Should().BeEquivalentTo([first.Id, second.Id]);
        }

        [Theory]
        [InlineData("discipline")]
        [InlineData("category")]
        [InlineData("level")]
        public void AddCatalogEntry_WhenNameDiffersOnlyByCaseAndOuterSpaces_RejectsDuplicate(string kind)
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            AddCatalogEntry(store, kind, "Élite");

            // Act
            var action = () => AddCatalogEntry(store, kind, "  élite  ");

            // Assert
            action.Should().Throw<InvalidOperationException>();
            GetCatalogNames(store, kind).Should().Equal("Élite");
        }

        [Fact]
        public void AddDiscipline_WhenCategoriesAndLevelsExist_PreservesSharedCatalogs()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            var category = new Category(Guid.NewGuid(), "Kids");
            var level = new Level(Guid.NewGuid(), "Open", false);
            store.AddCategory(category);
            store.AddLevel(level);

            // Act
            store.AddDiscipline(new Discipline(Guid.NewGuid(), "Danza"));

            // Assert
            store.GetCategories().Should().ContainSingle().Which.Id.Should().Be(category.Id);
            store.GetLevels().Should().ContainSingle().Which.Id.Should().Be(level.Id);
        }

        [Fact]
        public void Registry_WhenUsingDifferentArchives_KeepsDataIsolated()
        {
            // Arrange
            using var first = new RegistryArchive();
            using var second = new RegistryArchive();
            first.Create();
            second.Create();
            new SqliteRegistryStore(first.DatabasePath).AddAthlete(new Athlete(Guid.NewGuid(), "Anna", "Rossi"));
            var store = new SqliteRegistryStore(second.DatabasePath);

            // Act
            var athletes = store.GetAthletes();

            // Assert
            athletes.Should().BeEmpty();
        }

        [Fact]
        public void GetAthletes_WhenArchiveNeedsUpgrade_DoesNotMigrateImplicitly()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.CreateLegacy();
            var before = File.ReadAllBytes(archive.DatabasePath);
            var store = new SqliteRegistryStore(archive.DatabasePath);

            // Act
            var action = () => store.GetAthletes();

            // Assert
            action.Should().Throw<InvalidOperationException>();
            File.ReadAllBytes(archive.DatabasePath).Should().Equal(before);
            archive.Backups.Should().BeEmpty();
        }

        [Fact]
        public void Registry_WhenCatalogNamesMatchAcrossKinds_KeepsIndependentEntries()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            store.AddDiscipline(new Discipline(Guid.NewGuid(), "Open"));
            store.AddCategory(new Category(Guid.NewGuid(), "Open"));

            // Act
            store.AddLevel(new Level(Guid.NewGuid(), "Open", false));

            // Assert
            store.GetDisciplines().Should().ContainSingle();
            store.GetCategories().Should().ContainSingle();
            store.GetLevels().Should().ContainSingle().Which.IsPro.Should().BeFalse();
        }

        [Fact]
        public void AddAthlete_WhenArchiveIsForeign_ThrowsWithoutChangingFile()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            archive.Execute("PRAGMA application_id = 0;");
            var before = File.ReadAllBytes(archive.DatabasePath);
            var store = new SqliteRegistryStore(archive.DatabasePath);
            var athlete = new Athlete(Guid.NewGuid(), "Anna", "Rossi");

            // Act
            var action = () => store.AddAthlete(athlete);

            // Assert
            action.Should().Throw<InvalidDataException>();
            File.ReadAllBytes(archive.DatabasePath).Should().Equal(before);
        }

        private static void AddCatalogEntry(IRegistryStore store, string kind, string name)
        {
            switch (kind)
            {
                case "discipline":
                    store.AddDiscipline(new Discipline(Guid.NewGuid(), name));
                    break;
                case "category":
                    store.AddCategory(new Category(Guid.NewGuid(), name));
                    break;
                case "level":
                    store.AddLevel(new Level(Guid.NewGuid(), name, false));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static IEnumerable<string> GetCatalogNames(IRegistryStore store, string kind) => kind switch
        {
            "discipline" => store.GetDisciplines().Select(discipline => discipline.Name),
            "category" => store.GetCategories().Select(category => category.Name),
            "level" => store.GetLevels().Select(level => level.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
