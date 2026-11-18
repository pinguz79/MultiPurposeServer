using FluentAssertions;

using Microsoft.Data.Sqlite;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class RaceColorTests
    {
        [Fact]
        public void SetColor_WhenAlreadyAssigned_RejectsDuplicateAndPreservesPreviousColor()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var discipline = new Discipline(Guid.NewGuid(), "Moderno");
            archive.Registry.AddDiscipline(discipline);
            archive.Store.AddChoreography(archive.Entry("Seconda", discipline: discipline.Id));
            var store = new SqliteRaceStore(archive.DatabasePath);
            var keys = store.GetRaces().Select(race => race.Key).ToArray();
            store.SetColor(keys[0], "Rosa");
            store.SetColor(keys[1], "Pesca");

            // Act
            var action = () => store.SetColor(keys[1], "Rosa");

            // Assert
            action.Should().Throw<InvalidOperationException>();
            new SqliteRaceStore(archive.DatabasePath).GetColors()[keys[1]].Should().Be("Pesca");
        }

        [Fact]
        public void SetColor_WhenCleared_RemovesAssignment()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var store = new SqliteRaceStore(archive.DatabasePath);
            var key = store.GetRaces().Single().Key;
            store.SetColor(key, "Rosa");

            // Act
            store.SetColor(key, null);

            // Assert
            store.GetColors().Should().BeEmpty();
        }

        [Fact]
        public void SetColor_WhenUnknown_RejectsValue()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var store = new SqliteRaceStore(archive.DatabasePath);
            var key = store.GetRaces().Single().Key;

            // Act
            var action = () => store.SetColor(key, "Inventato");

            // Assert
            action.Should().Throw<ArgumentException>();
            store.GetColors().Should().BeEmpty();
        }

        [Fact]
        public void UpdateChoreography_WhenLastEntryMoves_ReleasesPreviousRaceColor()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            var store = new SqliteRaceStore(archive.DatabasePath);
            store.SetColor(store.GetRaces().Single().Key, "Rosa");
            var discipline = new Discipline(Guid.NewGuid(), "Moderno");
            archive.Registry.AddDiscipline(discipline);

            // Act
            archive.Store.UpdateChoreography(archive.Entry(id: entry.Id, discipline: discipline.Id));

            // Assert
            store.GetColors().Should().BeEmpty();
        }

        [Fact]
        public void Upgrade_WhenVersionEight_PreservesChoreographiesAndAddsColors()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            using (var connection = new SqliteConnection($"Data Source={archive.DatabasePath};Pooling=False"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "ALTER TABLE EventState DROP COLUMN BlockSize; DROP TABLE RaceColor; PRAGMA user_version = 8;";
                command.ExecuteNonQuery();
            }

            var store = new SqliteManifestationStore(archive.DatabasePath);

            // Act
            store.Upgrade(UpgradeBackupMode.SkipBackup);

            // Assert
            archive.Store.GetChoreographies().Should().ContainSingle();
            new SqliteRaceStore(archive.DatabasePath).GetColors().Should().BeEmpty();
            store.NeedsUpgrade().Should().BeFalse();
        }
    }
}
