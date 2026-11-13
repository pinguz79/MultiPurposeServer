using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Rankings;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class RankingScoreCardTests
    {
        [Fact]
        public Task ShowEntry_WhenJudgeExcluded_ShowsReasonAndOriginalScores() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            var judge = archive.Registry.GetJudges()[0];
            var criterion = archive.Registry.GetCriteria()[0];
            var entry = new RankingEntry(Guid.NewGuid(), 1, 1, 1, true, PerformanceStatus.Active, true, 1, 0, false,
                [new Score(Guid.NewGuid(), judge.Id, criterion.Id, 80)], [judge.Id]);
            using var card = new RankingScoreCard();

            // Act
            card.ShowEntry(entry, "Coreografia di prova", archive.Registry);

            // Assert
            var judgeCard = card.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<FlowLayoutPanel>().Single();
            judgeCard.ForeColor.Should().Be(Color.DimGray);
            judgeCard.Controls.OfType<Label>().Should().Contain(label => label.Text == "Giudice escluso da questa classifica");
            judgeCard.Controls.OfType<Label>().Should().Contain(label => label.Text == $"{criterion.Name}: {ScoreValue.Format(80)}");
            card.Controls.OfType<Label>().Single().Text.Should().Contain("Totale in questa classifica: " + ScoreValue.Format(0));
        });

        [Fact]
        public Task Clear_WhenSelectionRemoved_RemovesPreviousJudgeCards() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var card = new RankingScoreCard();
            card.ShowEntry(archive.Races.GetRanking(archive.Races.GetRaces()[0].Key).Entries[0], "Prova", archive.Registry);

            // Act
            card.Clear();

            // Assert
            card.Controls.OfType<FlowLayoutPanel>().Single().Controls.Count.Should().Be(0);
            card.Controls.OfType<Label>().Single().Text.Should().StartWith("Selezionare");
        });
    }
}
