using FluentAssertions;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class RegistryDeletionTests
    {
        [Fact]
        public void DeleteSchool_WhenUsed_RejectsWithoutDeletingChoreography()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());

            // Act
            var action = () => archive.Registry.DeleteSchool(archive.SchoolId);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Registry.GetSchools().Should().ContainSingle();
            archive.Store.GetChoreographies().Should().ContainSingle();
        }

        [Fact]
        public void DeleteSchool_WhenUnused_RemovesSchoolOnly()
        {
            // Arrange
            using var archive = new ChoreographyArchive();

            // Act
            archive.Registry.DeleteSchool(archive.SchoolId);

            // Assert
            archive.Registry.GetSchools().Should().BeEmpty();
            archive.Registry.GetAthletes().Should().HaveCount(3);
        }

        [Fact]
        public void DeleteAthlete_WhenUsed_RejectsWithoutChangingMembers()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());

            // Act
            var action = () => archive.Registry.DeleteAthlete(archive.Athletes[0]);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Store.GetChoreographies()[0].AthleteIds.Should().Contain(archive.Athletes[0]);
            archive.Registry.GetAthletes().Should().HaveCount(3);
        }

        [Fact]
        public void DeleteAthlete_WhenUnused_RemovesOnlySelectedIdentity()
        {
            // Arrange
            using var archive = new ChoreographyArchive();

            // Act
            archive.Registry.DeleteAthlete(archive.Athletes[0]);

            // Assert
            archive.Registry.GetAthletes().Select(entry => entry.Id).Should().BeEquivalentTo(archive.Athletes.Skip(1));
        }

        [Fact]
        public void DeleteChoreography_WhenPrepared_PreservesAthletesAndOtherBlocksAndCompactsOrder()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var first = archive.Entry("A");
            var second = archive.Entry("B");
            archive.Store.AddChoreography(first);
            archive.Store.AddChoreography(second);
            var store = new SqlitePerformanceStore(archive.DatabasePath);
            store.Prepare(1);
            store.ConfirmBlocks();

            // Act
            archive.Store.DeleteChoreography(first.Id);

            // Assert
            archive.Registry.GetAthletes().Should().HaveCount(3);
            archive.Store.GetChoreographies().Should().ContainSingle().Which.Id.Should().Be(second.Id);
            store.GetSchedule().Performances.Should().ContainSingle().Which.Should().Be(new Performance(second.Id, 2, 2, 1, false, PerformanceStatus.Active, false));
            store.GetSchedule().ConfirmedBlocks.Should().Equal(2);
        }

        [Fact]
        public void DeleteChoreography_WhenLastInRace_ReleasesRaceColorAndKeepsRegistry()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            var races = new SqliteRaceStore(archive.DatabasePath);
            races.SetColor(races.GetRaces()[0].Key, "Rosa");

            // Act
            archive.Store.DeleteChoreography(entry.Id);

            // Assert
            races.GetRaces().Should().BeEmpty();
            races.GetColors().Should().BeEmpty();
            archive.Registry.GetAthletes().Should().HaveCount(3);
            archive.Registry.GetSchools().Should().ContainSingle();
        }

        [Fact]
        public void DeleteChoreography_WhenBlockRetainsOtherEntries_RequiresBlockReconfirmation()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var first = archive.Entry("A");
            archive.Store.AddChoreography(first);
            archive.Store.AddChoreography(archive.Entry("B"));
            var performances = new SqlitePerformanceStore(archive.DatabasePath);
            performances.Prepare(5);
            performances.ConfirmBlocks();

            // Act
            archive.Store.DeleteChoreography(first.Id);

            // Assert
            performances.GetSchedule().ConfirmedBlocks.Should().BeEmpty();
            performances.GetSchedule().Performances.Should().ContainSingle().Which.Order.Should().Be(1);
        }

        [Fact]
        public void DeleteChoreography_WhenStarted_RejectsWithoutChangingVotes()
        {
            // Arrange
            using var archive = new AwardArchive();
            var scores = archive.Performances.OpenPage(1).Scores;

            // Act
            var action = () => archive.Choreographies.DeleteChoreography(archive.WinnerId);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Choreographies.GetChoreographies().Should().Contain(entry => entry.Id == archive.WinnerId);
            archive.Performances.OpenPage(1).Scores.Should().BeEquivalentTo(scores);
        }
    }
}
