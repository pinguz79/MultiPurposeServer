using FluentAssertions;

using Microsoft.Data.Sqlite;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class ArchiveUpgradeTests
    {
        [Fact]
        public void Upgrade_WhenVersionIsFour_PreservesJudgesAndAddsEmptySchedule()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var registry = new SqliteRegistryStore(archive.DatabasePath);
            var judge = new Judge(Guid.NewGuid(), "Giudice");
            registry.AddJudge(judge);
            archive.Execute("DROP TABLE AbsoluteAward; DROP TABLE IntermediateAward; DROP TABLE AwardSettings; ALTER TABLE Choreography DROP COLUMN Status; ALTER TABLE RaceConfiguration DROP COLUMN IsClosed;");
            archive.Execute("DROP TABLE Score; DROP TABLE PerformanceJudge; DROP TABLE Performance; DROP TABLE PerformanceBlock; DROP TABLE EventState; ALTER TABLE Judge DROP COLUMN IsAbsent; PRAGMA user_version = 4;");
            var store = new SqliteManifestationStore(archive.DatabasePath);

            // Act
            store.Upgrade();

            // Assert
            registry.GetJudges().Should().ContainSingle().Which.Should().BeEquivalentTo(judge);
            new SqlitePerformanceStore(archive.DatabasePath).GetSchedule().Performances.Should().BeEmpty();
            store.NeedsUpgrade().Should().BeFalse();
        }

        [Fact]
        public void Upgrade_WhenVersionIsThree_PreservesChoreographiesAndDerivesRaces()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = archive.DatabasePath, Pooling = false }.ToString()))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "DROP TABLE AbsoluteAward; DROP TABLE IntermediateAward; DROP TABLE AwardSettings; ALTER TABLE Choreography DROP COLUMN Status; DROP TABLE Score; DROP TABLE PerformanceJudge; DROP TABLE Performance; DROP TABLE PerformanceBlock; DROP TABLE EventState; DROP TABLE RaceJudge; DROP TABLE RaceConfiguration; DROP TABLE Judge; DROP TABLE EvaluationCriterion; PRAGMA user_version = 3;";
                command.ExecuteNonQuery();
            }

            var store = new SqliteManifestationStore(archive.DatabasePath);

            // Act
            store.Upgrade();

            // Assert
            archive.Store.GetChoreographies().Should().ContainSingle().Which.Should().BeEquivalentTo(entry);
            new SqliteRaceStore(archive.DatabasePath).GetRaces().Should().ContainSingle().Which.ChoreographyIds.Should().Equal(entry.Id);
            archive.Registry.GetCriteria().Should().HaveCount(3);
            store.NeedsUpgrade().Should().BeFalse();
        }

        [Theory]
        [InlineData(UpgradeBackupMode.CreateBackup)]
        [InlineData(UpgradeBackupMode.SkipBackup)]
        public void Upgrade_WhenVersionIsTwo_PreservesRegistryAndAddsChoreographySchema(UpgradeBackupMode mode)
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var registry = new SqliteRegistryStore(archive.DatabasePath);
            var athlete = new Athlete(Guid.NewGuid(), "Anna", "Rossi");
            registry.AddAthlete(athlete);
            archive.Execute("DROP TABLE AbsoluteAward; DROP TABLE IntermediateAward; DROP TABLE AwardSettings;");
            archive.Execute("DROP TABLE Score; DROP TABLE PerformanceJudge; DROP TABLE Performance; DROP TABLE PerformanceBlock; DROP TABLE EventState;");
            archive.Execute("DROP TABLE RaceJudge; DROP TABLE RaceConfiguration; DROP TABLE Judge; DROP TABLE EvaluationCriterion;");
            archive.Execute("DROP TABLE ChoreographyAthlete; DROP TABLE Choreography; DROP TABLE School; PRAGMA user_version = 2;");
            var store = new SqliteManifestationStore(archive.DatabasePath);

            // Act
            store.Upgrade(mode);

            // Assert
            store.NeedsUpgrade().Should().BeFalse();
            registry.GetAthletes().Should().ContainSingle().Which.Should().BeEquivalentTo(athlete);
            registry.GetSchools().Should().BeEmpty();
            new SqliteChoreographyStore(archive.DatabasePath).GetChoreographies().Should().BeEmpty();
            archive.Backups.Should().HaveCount(mode == UpgradeBackupMode.CreateBackup ? 1 : 0);
        }

        [Fact]
        public void Load_WhenVersionIsOne_ReadsWithoutChangingFile()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.CreateLegacy();
            var before = File.ReadAllBytes(archive.DatabasePath);
            var store = new SqliteManifestationStore(archive.DatabasePath);

            // Act
            var manifestation = store.Load();

            // Assert
            manifestation.Name.Should().Be("Manifestazione precedente");
            File.ReadAllBytes(archive.DatabasePath).Should().Equal(before);
            archive.Backups.Should().BeEmpty();
        }

        [Fact]
        public void Upgrade_WhenVersionIsOne_PreservesManifestationAndCreatesReadableBackup()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.CreateLegacy();
            var store = new SqliteManifestationStore(archive.DatabasePath);
            var expected = store.Load();

            // Act
            store.Upgrade();

            // Assert
            store.Load().Should().BeEquivalentTo(expected);
            store.NeedsUpgrade().Should().BeFalse();
            var backup = new SqliteManifestationStore(archive.Backups.Should().ContainSingle().Subject);
            backup.Load().Should().BeEquivalentTo(expected);
            backup.NeedsUpgrade().Should().BeTrue();
            new SqliteRegistryStore(archive.DatabasePath).GetAthletes().Should().BeEmpty();
        }

        [Fact]
        public void Upgrade_WhenBackupIsExplicitlySkipped_UpdatesWithoutCreatingCopy()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.CreateLegacy();
            var store = new SqliteManifestationStore(archive.DatabasePath);
            var expected = store.Load();

            // Act
            store.Upgrade(UpgradeBackupMode.SkipBackup);

            // Assert
            store.Load().Should().BeEquivalentTo(expected);
            store.NeedsUpgrade().Should().BeFalse();
            archive.Backups.Should().BeEmpty();
            new SqliteRegistryStore(archive.DatabasePath).GetAthletes().Should().BeEmpty();
        }

        [Fact]
        public void Upgrade_WhenSkippingBackupAndSchemaFails_RollsBackWithoutCreatingCopy()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.CreateLegacy();
            archive.Execute("CREATE TABLE Category (Id TEXT);");
            var store = new SqliteManifestationStore(archive.DatabasePath);
            var expected = store.Load();

            // Act
            var action = () => store.Upgrade(UpgradeBackupMode.SkipBackup);

            // Assert
            action.Should().Throw<SqliteException>();
            store.NeedsUpgrade().Should().BeTrue();
            store.Load().Should().BeEquivalentTo(expected);
            archive.Backups.Should().BeEmpty();
        }

        [Fact]
        public void Upgrade_WhenAlreadyCurrent_DoesNotChangeFileOrCreateBackup()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var before = File.ReadAllBytes(archive.DatabasePath);
            var store = new SqliteManifestationStore(archive.DatabasePath);

            // Act
            store.Upgrade();

            // Assert
            File.ReadAllBytes(archive.DatabasePath).Should().Equal(before);
            archive.Backups.Should().BeEmpty();
        }

        [Fact]
        public void Upgrade_WhenSchemaCreationFails_RollsBackAndRetainsBackup()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.CreateLegacy();
            archive.Execute("CREATE TABLE Category (Id TEXT);");
            var store = new SqliteManifestationStore(archive.DatabasePath);
            var expected = store.Load();

            // Act
            var action = () => store.Upgrade();

            // Assert
            action.Should().Throw<SqliteException>();
            store.NeedsUpgrade().Should().BeTrue();
            store.Load().Should().BeEquivalentTo(expected);
            archive.Backups.Should().ContainSingle();
            // Una tabella creata prima del conflitto non deve sopravvivere al rollback.
            var createAthleteTable = () => archive.Execute("CREATE TABLE Athlete (Id TEXT);");
            createAthleteTable.Should().NotThrow();
        }
    }
}
