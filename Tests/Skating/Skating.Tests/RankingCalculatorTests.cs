using FluentAssertions;

using Skating.Core;

namespace Skating.Tests
{
    public sealed class RankingCalculatorTests
    {
        [Fact]
        public void Calculate_WhenJuryCountsDiffer_ExcludesOnlyNonCommonJudges()
        {
            // Arrange
            var judges = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
            var first = Entry(1, judges);
            var second = Entry(2, judges.Take(3));

            // Act
            var result = RankingCalculator.Calculate(Key(), false, [first, second]);

            // Assert
            result.Entries.Should().OnlyContain(entry => entry.TotalTenths == 240 && entry.Position == 1);
            result.Entries[0].ExcludedJudgeIds.Should().Equal(judges[3]);
            result.Entries[0].Scores.Should().HaveCount(4);
            result.Entries[1].ExcludedJudgeIds.Should().BeEmpty();
        }

        [Fact]
        public void Calculate_WhenCountsEqualButIdentitiesDiffer_DoesNotExcludeJudges()
        {
            // Arrange
            var first = Entry(1, [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
            var second = Entry(2, [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);

            // Act
            var result = RankingCalculator.Calculate(Key(), false, [first, second]);

            // Assert
            result.Entries.Should().OnlyContain(entry => entry.TotalTenths == 240 && entry.ExcludedJudgeIds.Count == 0);
        }

        [Theory]
        [InlineData(PerformanceStatus.NotPresented)]
        [InlineData(PerformanceStatus.Disqualified)]
        public void Calculate_WhenExcludedEntryHasFewerVotes_DoesNotAffectEligibleScores(PerformanceStatus status)
        {
            // Arrange
            var judges = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
            var first = Entry(1, judges);
            var excluded = Entry(2, judges.Take(2)) with { Status = status };

            // Act
            var result = RankingCalculator.Calculate(Key(), false, [first, excluded]);

            // Assert
            result.Entries[0].TotalTenths.Should().Be(320);
            result.Entries[1].Position.Should().BeNull();
            result.Entries[1].TotalTenths.Should().BeNull();
        }

        [Fact]
        public void Calculate_WhenIncompleteEntryHasFewerVotes_DoesNotSimulateAbsence()
        {
            // Arrange
            var judges = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
            var first = Entry(1, judges);
            var incomplete = Entry(2, judges.Take(1)) with { IsComplete = false, IsConfirmed = false };

            // Act
            var result = RankingCalculator.Calculate(Key(), false, [first, incomplete]);

            // Assert
            result.Entries[0].TotalTenths.Should().Be(320);
            result.Entries[1].Position.Should().BeNull();
            result.CanClose.Should().BeFalse();
        }

        [Theory]
        [InlineData(2, true)]
        [InlineData(3, false)]
        public void Calculate_WhenTiedAfterLeadingEntries_DetectsPodiumBoundary(int leaders, bool podium)
        {
            // Arrange
            var judge = Guid.NewGuid();
            var entries = Enumerable.Range(0, leaders).Select(index => Entry(index, [judge], 100 - index)).ToList();
            entries.Add(Entry(leaders, [judge], 70));
            entries.Add(Entry(leaders + 1, [judge], 70));
            entries.Add(Entry(leaders + 2, [judge], 60));

            // Act
            var result = RankingCalculator.Calculate(Key(), false, entries);

            // Assert
            result.HasPodiumTies.Should().Be(podium);
            result.CanClose.Should().Be(!podium);
            result.Entries[leaders].Position.Should().Be(leaders + 1);
            result.Entries[leaders + 1].Position.Should().Be(leaders + 1);
            result.Entries[leaders + 2].Position.Should().Be(leaders + 3);
        }

        [Fact]
        public void Calculate_WhenCompleteOpenEntry_HasPositionButCannotClose()
        {
            // Arrange
            var entry = Entry(1, [Guid.NewGuid()], 0) with { IsConfirmed = false };

            // Act
            var result = RankingCalculator.Calculate(Key(), false, [entry]);

            // Assert
            result.Entries[0].Position.Should().Be(1);
            result.Entries[0].TotalTenths.Should().Be(0);
            result.CanClose.Should().BeFalse();
        }

        private static RaceKey Key() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        private static RankingEntry Entry(int order, IEnumerable<Guid> judges, int value = 80)
        {
            var id = Guid.NewGuid();
            return new RankingEntry(id, order, order, 1, true, PerformanceStatus.Active, true, null, null, false,
                [.. judges.Select(judge => new Score(id, judge, Guid.NewGuid(), value))], []);
        }
    }
}
