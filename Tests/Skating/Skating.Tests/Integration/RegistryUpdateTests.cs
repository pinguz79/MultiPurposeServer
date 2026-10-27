using FluentAssertions;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class RegistryUpdateTests
    {
        [Theory]
        [InlineData("athlete")]
        [InlineData("school")]
        [InlineData("discipline")]
        [InlineData("category")]
        [InlineData("level")]
        public void Update_WhenEntityExists_PreservesIdentityAndPersistsChanges(string kind)
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            var id = Guid.NewGuid();
            Add(store, kind, id, "Originale");

            // Act
            Update(store, kind, id, "Modificato");

            // Assert
            var reopened = new SqliteRegistryStore(archive.DatabasePath);
            Entries(reopened, kind).Should().Equal((id, "Modificato"));
            if (kind == "athlete")
            {
                reopened.GetAthletes().Single().LastName.Should().Be("Cognome modificato");
            }

            if (kind == "level")
            {
                reopened.GetLevels().Single().IsPro.Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("athlete")]
        [InlineData("discipline")]
        [InlineData("category")]
        [InlineData("level")]
        public void Update_WhenIdIsMissing_ThrowsWithoutInserting(string kind)
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            var id = Guid.NewGuid();

            // Act
            var action = () => Update(store, kind, id, "Modificato");

            // Assert
            action.Should().Throw<InvalidOperationException>();
            Entries(store, kind).Should().BeEmpty();
        }

        [Theory]
        [InlineData("discipline")]
        [InlineData("category")]
        [InlineData("level")]
        public void Update_WhenNameBelongsToAnotherEntry_RejectsWithoutChangingOriginal(string kind)
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            var id = Guid.NewGuid();
            var other = Guid.NewGuid();
            Add(store, kind, id, "Originale");
            Add(store, kind, other, "Élite");

            // Act
            var action = () => Update(store, kind, id, "  élite  ");

            // Assert
            action.Should().Throw<InvalidOperationException>();
            Entries(store, kind).Should().BeEquivalentTo([(id, "Originale"), (other, "Élite")]);
            if (kind == "level")
            {
                store.GetLevels().Single(level => level.Id == id).IsPro.Should().BeFalse();
            }
        }

        [Theory]
        [InlineData("discipline")]
        [InlineData("category")]
        [InlineData("level")]
        public void Update_WhenNameIsUnchanged_DoesNotConflictWithItself(string kind)
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteRegistryStore(archive.DatabasePath);
            var id = Guid.NewGuid();
            Add(store, kind, id, "Nome");

            // Act
            Update(store, kind, id, "Nome");

            // Assert
            Entries(store, kind).Should().Equal((id, "Nome"));
        }

        private static void Add(IRegistryStore store, string kind, Guid id, string name)
        {
            switch (kind)
            {
                case "athlete": store.AddAthlete(new Athlete(id, name, "Cognome")); break;
                case "school": store.AddSchool(new School(id, name)); break;
                case "discipline": store.AddDiscipline(new Discipline(id, name)); break;
                case "category": store.AddCategory(new Category(id, name)); break;
                case "level": store.AddLevel(new Level(id, name, false)); break;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static void Update(IRegistryStore store, string kind, Guid id, string name)
        {
            switch (kind)
            {
                case "athlete": store.UpdateAthlete(new Athlete(id, name, "Cognome modificato")); break;
                case "school": store.UpdateSchool(new School(id, name)); break;
                case "discipline": store.UpdateDiscipline(new Discipline(id, name)); break;
                case "category": store.UpdateCategory(new Category(id, name)); break;
                case "level": store.UpdateLevel(new Level(id, name, true)); break;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static IEnumerable<(Guid Id, string Name)> Entries(IRegistryStore store, string kind) => kind switch
        {
            "athlete" => store.GetAthletes().Select(athlete => (athlete.Id, athlete.FirstName)),
            "school" => store.GetSchools().Select(school => (school.Id, school.Name)),
            "discipline" => store.GetDisciplines().Select(discipline => (discipline.Id, discipline.Name)),
            "category" => store.GetCategories().Select(category => (category.Id, category.Name)),
            "level" => store.GetLevels().Select(level => (level.Id, level.Name)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
