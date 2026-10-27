using FluentAssertions;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class PerformanceStoreTests
    {
        [Fact]
        public void SaveScore_WhenAbsentOverride_RequiresRemainingCriteria()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            var store = Start(archive);
            var judge = archive.Registry.GetJudges()[0].Id;
            var criterion = archive.Registry.GetCriteria()[0].Id;
            store.SetJudgeRequired(entry.Id, judge, false);

            // Act
            store.SaveScore(entry.Id, judge, criterion, 80, allowAbsent: true);

            // Assert
            var page = store.OpenPage(1);
            page.Judges.Single(item => item.JudgeId == judge).IsRequired.Should().BeTrue();
            ScoreCompletion.IsComplete(entry.Id, page, [.. archive.Registry.GetCriteria().Select(item => item.Id)]).Should().BeFalse();
        }

        [Fact]
        public void SetJudgeRequired_WhenScoresExist_DoesNotEraseThem()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            var store = Start(archive);
            var judge = archive.Registry.GetJudges()[0].Id;
            store.SaveScore(entry.Id, judge, archive.Registry.GetCriteria()[0].Id, 80);

            // Act
            var action = () => store.SetJudgeRequired(entry.Id, judge, false);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            store.OpenPage(1).Scores.Should().ContainSingle();
        }

        [Fact]
        public void Move_WhenConfirmed_PreservesNumbersAndBlocks()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry("Prima"));
            archive.Store.AddChoreography(archive.Entry("Seconda"));
            var store = Start(archive);
            var previous = store.GetSchedule().Performances[0];

            // Act
            store.Move(previous.ChoreographyId, 1);

            // Assert
            store.GetSchedule().Performances[1].Should().Be(previous with { Order = 2 });
        }

        [Fact]
        public void Reopen_WhenConfirmed_PreservesScoresAndEnablesEditing()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            var store = Start(archive);
            foreach (var judge in archive.Registry.GetJudges())
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    store.SaveScore(entry.Id, judge.Id, criterion.Id, 70);
                }
            }

            store.ConfirmComplete(1);

            // Act
            store.Reopen(entry.Id);

            // Assert
            store.GetSchedule().Performances.Single().IsConfirmed.Should().BeFalse();
            store.OpenPage(1).Scores.Should().HaveCount(9).And.OnlyContain(score => score.Tenths == 70);
        }

        [Fact]
        public void ConfirmComplete_WhenOnlyOneRowComplete_ConfirmsOnlyThatRowAndCreatesBackup()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            archive.Store.AddChoreography(archive.Entry("Seconda"));
            var store = Start(archive);
            foreach (var judge in archive.Registry.GetJudges())
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    store.SaveScore(entry.Id, judge.Id, criterion.Id, 0);
                }
            }

            // Act
            var confirmed = store.ConfirmComplete(1);

            // Assert
            confirmed.Should().Be(1);
            store.GetSchedule().Performances.Single(row => row.ChoreographyId == entry.Id).IsConfirmed.Should().BeTrue();
            store.GetSchedule().Performances.Count(row => !row.IsConfirmed).Should().Be(1);
            Directory.GetFiles(Path.GetDirectoryName(archive.DatabasePath) ?? throw new InvalidOperationException(), "*.before-confirm-*.bak").Should().ContainSingle();
        }

        [Fact]
        public void SaveScore_WhenConfirmed_RejectsChange()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            var store = Start(archive);
            foreach (var judge in archive.Registry.GetJudges())
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    store.SaveScore(entry.Id, judge.Id, criterion.Id, 70);
                }
            }

            store.ConfirmComplete(1);
            var firstJudge = archive.Registry.GetJudges()[0].Id;
            var firstCriterion = archive.Registry.GetCriteria()[0].Id;

            // Act
            var action = () => store.SaveScore(entry.Id, firstJudge, firstCriterion, 80);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            store.OpenPage(1).Scores.Should().OnlyContain(score => score.Tenths == 70);
        }

        [Fact]
        public void OpenPage_WhenGlobalAbsenceChanges_PreservesExistingSnapshot()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var store = Start(archive);
            var judge = archive.Registry.GetJudges()[0];
            archive.Registry.UpdateJudge(new Judge(judge.Id, judge.Name, true));

            // Act
            var page = store.OpenPage(1);

            // Assert
            page.Judges.Should().OnlyContain(entry => entry.IsRequired);
        }

        [Fact]
        public void AddChoreography_WhenStarted_AppendsUnconfirmedSupplementaryBlock()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var store = Start(archive);
            var before = store.GetSchedule().Performances.Single();
            var added = archive.Entry("Nuova");

            // Act
            archive.Store.AddChoreography(added);

            // Assert
            store.GetSchedule().Performances[0].Should().Be(before);
            var appended = store.GetSchedule().Performances.Single(entry => entry.ChoreographyId == added.Id);
            appended.Number.Should().Be(2);
            store.GetSchedule().ConfirmedBlocks.Should().NotContain(appended.Block);
        }

        [Fact]
        public void AddCriterion_WhenStarted_RejectsChange()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            Start(archive);

            // Act
            var action = () => archive.Registry.AddCriterion(new EvaluationCriterion(Guid.NewGuid(), "Altro"));

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void SaveScore_WhenCleared_RemovesScoreWithoutBackup()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();
            archive.Store.AddChoreography(entry);
            var store = Start(archive);
            var judge = archive.Registry.GetJudges()[0].Id;
            var criterion = archive.Registry.GetCriteria()[0].Id;
            store.SaveScore(entry.Id, judge, criterion, 90);

            // Act
            store.SaveScore(entry.Id, judge, criterion, null);

            // Assert
            store.OpenPage(1).Scores.Should().BeEmpty();
            Directory.GetFiles(Path.GetDirectoryName(archive.DatabasePath) ?? throw new InvalidOperationException(), "*.before-confirm-*.bak").Should().BeEmpty();
        }

        private static SqlitePerformanceStore Start(ChoreographyArchive archive)
        {
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
    }
}
