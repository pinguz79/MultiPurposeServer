using FluentAssertions;

using Microsoft.Data.Sqlite;

using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class SchedulePreparationTests
    {
        [Fact]
        public void Prepare_WhenTwelveEntriesAndSizeFive_CreatesFiveFiveTwoPreservingOrder()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            for (var index = 1; index <= 12; index++)
            {
                archive.Store.AddChoreography(archive.Entry($"Coreografia {index:00}"));
            }

            var store = new SqlitePerformanceStore(archive.DatabasePath);
            store.Prepare(3);
            store.Move(store.GetSchedule().Performances[0].ChoreographyId, 1);
            store.ConfirmBlocks();
            var order = store.GetSchedule().Performances.Select(entry => entry.ChoreographyId).ToArray();

            // Act
            store.Prepare(5);

            // Assert
            var schedule = store.GetSchedule();
            schedule.Performances.GroupBy(entry => entry.Block).Select(group => group.Count()).Should().Equal(5, 5, 2);
            schedule.Performances.Select(entry => entry.ChoreographyId).Should().Equal(order);
            schedule.ConfirmedBlocks.Should().BeEmpty();
            new SqlitePerformanceStore(archive.DatabasePath).GetSchedule().BlockSize.Should().Be(5);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddChoreography_WhenSchedulePrepared_FillsOnlyUnconfirmedLastBlock(bool confirmed)
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry("Prima"));
            var store = new SqlitePerformanceStore(archive.DatabasePath);
            store.Prepare(5);
            if (confirmed)
            {
                store.ConfirmBlocks();
            }

            // Act
            for (var index = 0; index < 6; index++)
            {
                new SqliteChoreographyStore(archive.DatabasePath).AddChoreography(archive.Entry($"Nuova {index}"));
            }

            // Assert
            store.GetSchedule().Performances.GroupBy(entry => entry.Block).Select(group => group.Count()).Should().Equal(confirmed ? [1, 5, 1] : [5, 2]);
            store.GetSchedule().ConfirmedBlocks.Should().Equal(confirmed ? [1] : []);
        }

        [Fact]
        public void Prepare_WhenStarted_RejectsWithoutChangingScoresOrSchedule()
        {
            // Arrange
            using var archive = new AwardArchive();
            var schedule = archive.Performances.GetSchedule();
            var scores = archive.Performances.OpenPage(1).Scores;

            // Act
            var action = () => archive.Performances.Prepare(1);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Performances.GetSchedule().Should().BeEquivalentTo(schedule);
            archive.Performances.OpenPage(1).Scores.Should().BeEquivalentTo(scores);
        }

        [Fact]
        public void Upgrade_WhenVersionNine_PreservesBlocksAndColorsAndAddsDefaultCapacity()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var store = new SqlitePerformanceStore(archive.DatabasePath);
            store.Prepare(5);
            store.ConfirmBlocks();
            var races = new SqliteRaceStore(archive.DatabasePath);
            races.SetColor(races.GetRaces()[0].Key, "Rosa");
            var before = store.GetSchedule();
            using (var connection = new SqliteConnection($"Data Source={archive.DatabasePath};Pooling=False"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "ALTER TABLE EventState DROP COLUMN BlockSize; PRAGMA user_version = 9;";
                command.ExecuteNonQuery();
            }

            // Act
            new SqliteManifestationStore(archive.DatabasePath).Upgrade();

            // Assert
            store.GetSchedule().Should().BeEquivalentTo(before with { BlockSize = 20 });
            races.GetColors().Values.Should().Equal("Rosa");
            Directory.GetFiles(Path.GetDirectoryName(archive.DatabasePath) ?? throw new InvalidOperationException(), "*.before-v10-*.bak").Should().ContainSingle();
        }
    }
}
