using FluentAssertions;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class RaceStoreTests
    {
        [Fact]
        public void GetRaces_WhenNoChoreographies_DoesNotGenerateCartesianProduct()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var store = new SqliteRaceStore(archive.DatabasePath);

            // Act
            var races = store.GetRaces();

            // Assert
            races.Should().BeEmpty();
        }

        [Fact]
        public void GetRaces_WhenChoreographiesShareCombination_GroupsThemWithoutWriting()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            archive.Store.AddChoreography(archive.Entry("Seconda"));
            var before = File.ReadAllBytes(archive.DatabasePath);
            var store = new SqliteRaceStore(archive.DatabasePath);

            // Act
            var races = store.GetRaces();

            // Assert
            races.Should().ContainSingle().Which.ChoreographyIds.Should().HaveCount(2);
            File.ReadAllBytes(archive.DatabasePath).Should().Equal(before);
        }

        [Fact]
        public void UpdateChoreography_WhenLastEntryMoves_RemovesEmptyRaceAndPreservesRegistry()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            var discipline = new Discipline(Guid.NewGuid(), "Moderno");
            archive.Registry.AddDiscipline(discipline);

            // Act
            archive.Store.UpdateChoreography(archive.Entry(id: entry.Id, discipline: discipline.Id));

            // Assert
            new SqliteRaceStore(archive.DatabasePath).GetRaces().Should().ContainSingle().Which.Key.DisciplineId.Should().Be(discipline.Id);
            archive.Registry.GetDisciplines().Should().HaveCount(2);
            archive.Registry.GetAthletes().Should().HaveCount(3);
        }

        [Fact]
        public void SetJury_WhenValid_PersistsExplicitSelection()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var judges = AddJudges(archive.Registry, 4);
            var store = new SqliteRaceStore(archive.DatabasePath);
            var key = store.GetRaces().Single().Key;

            // Act
            store.SetJury(key, [.. judges.Take(3)]);

            // Assert
            var race = new SqliteRaceStore(archive.DatabasePath).GetRaces().Single();
            race.HasCustomJury.Should().BeTrue();
            race.JudgeIds.Should().BeEquivalentTo(judges.Take(3));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddJudge_WhenJuryExists_UpdatesOnlyDefaultJury(bool custom)
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var judges = AddJudges(archive.Registry, 3);
            var store = new SqliteRaceStore(archive.DatabasePath);
            if (custom)
            {
                store.SetJury(store.GetRaces().Single().Key, judges);
            }

            var added = new Judge(Guid.NewGuid(), "Nuovo");

            // Act
            archive.Registry.AddJudge(added);

            // Assert
            store.GetRaces().Single().JudgeIds.Contains(added.Id).Should().Be(!custom);
        }

        [Fact]
        public void SetJury_WhenTooFewJudges_LeavesDefaultUnchanged()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var judges = AddJudges(archive.Registry, 3);
            var store = new SqliteRaceStore(archive.DatabasePath);

            // Act
            var action = () => store.SetJury(store.GetRaces().Single().Key, [.. judges.Take(2)]);

            // Assert
            action.Should().Throw<ArgumentException>();
            store.GetRaces().Single().HasCustomJury.Should().BeFalse();
        }

        [Fact]
        public void SetJury_WhenUnknownJudge_RollsBackPreviousSelection()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var judges = AddJudges(archive.Registry, 4);
            var store = new SqliteRaceStore(archive.DatabasePath);
            var key = store.GetRaces().Single().Key;
            store.SetJury(key, [.. judges.Take(3)]);

            // Act
            var action = () => store.SetJury(key, [judges[2], judges[3], Guid.NewGuid()]);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            store.GetRaces().Single().JudgeIds.Should().BeEquivalentTo(judges.Take(3));
        }

        [Fact]
        public void UseDefaultJury_WhenCustom_RestoresAllJudges()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var judges = AddJudges(archive.Registry, 4);
            var store = new SqliteRaceStore(archive.DatabasePath);
            var key = store.GetRaces().Single().Key;
            store.SetJury(key, [.. judges.Take(3)]);

            // Act
            store.UseDefaultJury(key);

            // Assert
            store.GetRaces().Single().HasCustomJury.Should().BeFalse();
            store.GetRaces().Single().JudgeIds.Should().BeEquivalentTo(judges);
        }

        [Fact]
        public void SetJury_WhenRaceMissing_DoesNotCreateRace()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var judges = AddJudges(archive.Registry, 3);
            var store = new SqliteRaceStore(archive.DatabasePath);

            // Act
            var action = () => store.SetJury(new RaceKey(archive.DisciplineId, archive.CategoryId, archive.LevelId), judges);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            store.GetRaces().Should().BeEmpty();
        }

        [Fact]
        public void Create_WhenNewArchive_SeedsCommonCriteria()
        {
            // Arrange
            using var archive = new RegistryArchive();

            // Act
            archive.Create();

            // Assert
            new SqliteRegistryStore(archive.DatabasePath).GetCriteria().Select(entry => entry.Name).Should().BeEquivalentTo("Tecnica", "Interpretazione / Musicalità", "Show");
        }

        [Fact]
        public void DeleteCriterion_WhenExisting_RemovesOnlySelectedCriterion()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var criteria = archive.Registry.GetCriteria();

            // Act
            archive.Registry.DeleteCriterion(criteria[0].Id);

            // Assert
            archive.Registry.GetCriteria().Select(entry => entry.Id).Should().BeEquivalentTo(criteria.Skip(1).Select(entry => entry.Id));
        }

        private static Guid[] AddJudges(IRegistryStore registry, int count)
        {
            List<Guid> ids = [];
            for (var index = 0; index < count; index++)
            {
                var judge = new Judge(Guid.NewGuid(), $"Giudice {index}");
                registry.AddJudge(judge);
                ids.Add(judge.Id);
            }

            return [.. ids];
        }
    }
}
