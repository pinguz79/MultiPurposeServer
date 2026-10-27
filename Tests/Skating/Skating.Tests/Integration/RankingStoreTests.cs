using FluentAssertions;

using Microsoft.Data.Sqlite;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class RankingStoreTests
    {
        [Fact]
        public void GetRanking_WhenNotPrepared_ReturnsUnrankedEntriesWithoutWriting()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var races = new SqliteRaceStore(archive.DatabasePath);
            var key = races.GetRaces().Single().Key;
            var before = File.ReadAllBytes(archive.DatabasePath);

            // Act
            var result = races.GetRanking(key);

            // Assert
            result.Entries.Should().ContainSingle().Which.Position.Should().BeNull();
            result.CanClose.Should().BeFalse();
            File.ReadAllBytes(archive.DatabasePath).Should().Equal(before);
        }

        [Theory]
        [InlineData(PerformanceStatus.NotPresented)]
        [InlineData(PerformanceStatus.Disqualified)]
        public void SetStatus_WhenRevoked_PreservesVotesAndReopensSheet(PerformanceStatus status)
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            var races = new SqliteRaceStore(archive.DatabasePath);
            var key = races.GetRaces().Single().Key;
            var id = archive.Store.GetChoreographies()[0].Id;
            Fill(archive, performance, id, 80);
            performance.ConfirmComplete(1);
            races.SetStatus(id, status);

            // Act
            races.SetStatus(id, PerformanceStatus.Active);

            // Assert
            var row = races.GetRanking(key).Entries.Single();
            row.Status.Should().Be(PerformanceStatus.Active);
            row.IsConfirmed.Should().BeFalse();
            row.TotalTenths.Should().Be(720);
            row.Scores.Should().HaveCount(9);
        }

        [Fact]
        public void Close_WhenPodiumTied_RejectsClosure()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive, 2);
            var races = new SqliteRaceStore(archive.DatabasePath);
            foreach (var entry in archive.Store.GetChoreographies())
            {
                Fill(archive, performance, entry.Id, 80);
            }

            performance.ConfirmComplete(1);
            var key = races.GetRaces().Single().Key;

            // Act
            var action = () => races.Close(key);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            races.GetRanking(key).HasPodiumTies.Should().BeTrue();
            races.GetRanking(key).IsClosed.Should().BeFalse();
        }

        [Fact]
        public void Close_WhenCompleteButUnconfirmed_RejectsClosure()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            Fill(archive, performance, archive.Store.GetChoreographies()[0].Id, 80);
            var races = new SqliteRaceStore(archive.DatabasePath);
            var key = races.GetRaces().Single().Key;

            // Act
            var action = () => races.Close(key);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Reopen_WhenRaceClosed_RejectsSheetReopening()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            var id = archive.Store.GetChoreographies()[0].Id;
            Fill(archive, performance, id, 80);
            performance.ConfirmComplete(1);
            var races = new SqliteRaceStore(archive.DatabasePath);
            races.Close(races.GetRaces().Single().Key);

            // Act
            var action = () => performance.Reopen(id);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            performance.GetSchedule().Performances.Single().IsRaceClosed.Should().BeTrue();
        }

        [Fact]
        public void Reopen_WhenRaceClosed_PreservesConfirmedScores()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            Fill(archive, performance, archive.Store.GetChoreographies()[0].Id, 80);
            performance.ConfirmComplete(1);
            var races = new SqliteRaceStore(archive.DatabasePath);
            var key = races.GetRaces().Single().Key;
            races.Close(key);

            // Act
            races.Reopen(key);

            // Assert
            var result = races.GetRanking(key);
            result.IsClosed.Should().BeFalse();
            result.Entries.Single().IsConfirmed.Should().BeTrue();
            result.Entries.Single().TotalTenths.Should().Be(720);
        }

        [Fact]
        public void GetRanking_WhenGlobalAbsenceChanges_PreservesClosedRanking()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            Fill(archive, performance, archive.Store.GetChoreographies()[0].Id, 80);
            performance.ConfirmComplete(1);
            var races = new SqliteRaceStore(archive.DatabasePath);
            var key = races.GetRaces().Single().Key;
            races.Close(key);
            var judge = archive.Registry.GetJudges()[0];
            archive.Registry.UpdateJudge(new Judge(judge.Id, judge.Name, true));

            // Act
            var result = races.GetRanking(key);

            // Assert
            result.IsClosed.Should().BeTrue();
            result.Entries.Single().TotalTenths.Should().Be(720);
        }

        [Theory]
        [InlineData(PerformanceStatus.NotPresented)]
        [InlineData(PerformanceStatus.Disqualified)]
        public void SaveScore_WhenExcluded_RejectsEdit(PerformanceStatus status)
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            var id = archive.Store.GetChoreographies()[0].Id;
            new SqliteRaceStore(archive.DatabasePath).SetStatus(id, status);

            // Act
            var action = () => performance.SaveScore(id, archive.Registry.GetJudges()[0].Id, archive.Registry.GetCriteria()[0].Id, 70);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Upgrade_WhenVersionFive_PreservesScoresAndAddsRanking()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            Fill(archive, performance, archive.Store.GetChoreographies()[0].Id, 80);
            using (var connection = new SqliteConnection($"Data Source={archive.DatabasePath};Pooling=False"))
            {
                connection.Open();
                connection.CreateCollation("SKATING_NAME", (left, right) => string.Compare(left, right, StringComparison.OrdinalIgnoreCase));
                using var command = connection.CreateCommand();
                command.CommandText = "DROP TABLE AbsoluteAward; DROP TABLE IntermediateAward; DROP TABLE AwardSettings; ALTER TABLE Choreography DROP COLUMN Status; ALTER TABLE RaceConfiguration DROP COLUMN IsClosed; PRAGMA user_version = 5;";
                command.ExecuteNonQuery();
            }

            // Act
            new SqliteManifestationStore(archive.DatabasePath).Upgrade();

            // Assert
            var races = new SqliteRaceStore(archive.DatabasePath);
            races.GetRanking(races.GetRaces().Single().Key).Entries.Single().TotalTenths.Should().Be(720);
        }

        [Fact]
        public void SetStatus_WhenRaceClosed_RejectsChange()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            var id = archive.Store.GetChoreographies()[0].Id;
            Fill(archive, performance, id, 80);
            performance.ConfirmComplete(1);
            var races = new SqliteRaceStore(archive.DatabasePath);
            var key = races.GetRaces().Single().Key;
            races.Close(key);

            // Act
            var action = () => races.SetStatus(id, PerformanceStatus.Disqualified);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            races.GetRanking(key).Entries.Single().Status.Should().Be(PerformanceStatus.Active);
        }

        [Fact]
        public void AddChoreography_WhenRaceClosed_RejectsNewParticipant()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive);
            Fill(archive, performance, archive.Store.GetChoreographies()[0].Id, 80);
            performance.ConfirmComplete(1);
            var races = new SqliteRaceStore(archive.DatabasePath);
            races.Close(races.GetRaces().Single().Key);
            var entry = archive.Entry("Nuova");

            // Act
            var action = () => archive.Store.AddChoreography(entry);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Store.GetChoreographies().Should().ContainSingle();
        }

        [Fact]
        public void GetRanking_WhenJuriesDiffer_UsesHistoricalAttendance()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var performance = Prepare(archive, 2);
            var entries = archive.Store.GetChoreographies();
            var absent = archive.Registry.GetJudges()[0].Id;
            Fill(archive, performance, entries[0].Id, 80);
            performance.SetJudgeRequired(entries[1].Id, absent, false);
            foreach (var judge in archive.Registry.GetJudges().Where(judge => judge.Id != absent))
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    performance.SaveScore(entries[1].Id, judge.Id, criterion.Id, 70);
                }
            }

            var races = new SqliteRaceStore(archive.DatabasePath);
            var key = races.GetRaces().Single().Key;

            // Act
            var result = races.GetRanking(key);

            // Assert
            result.Entries[0].TotalTenths.Should().Be(480);
            result.Entries[0].ExcludedJudgeIds.Should().Equal(absent);
            result.Entries[1].TotalTenths.Should().Be(420);
        }

        private static SqlitePerformanceStore Prepare(ChoreographyArchive archive, int count = 1)
        {
            for (var index = 0; index < count; index++)
            {
                archive.Store.AddChoreography(archive.Entry($"Coreografia {index}"));
            }

            for (var index = 0; index < 3; index++)
            {
                archive.Registry.AddJudge(new Judge(Guid.NewGuid(), $"Giudice {index}"));
            }

            var store = new SqlitePerformanceStore(archive.DatabasePath);
            store.Prepare(20);
            store.ConfirmBlocks();
            store.Start();
            store.OpenPage(1);
            return store;
        }

        private static void Fill(ChoreographyArchive archive, SqlitePerformanceStore store, Guid id, int value)
        {
            foreach (var judge in archive.Registry.GetJudges())
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    store.SaveScore(id, judge.Id, criterion.Id, value);
                }
            }
        }
    }
}
