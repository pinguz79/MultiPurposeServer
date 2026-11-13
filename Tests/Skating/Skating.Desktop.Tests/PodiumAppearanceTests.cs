using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Rankings;

namespace Skating.Desktop.Tests
{
    public sealed class PodiumAppearanceTests
    {
        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, false)]
        [InlineData(null, false)]
        public void IsOnPodium_WhenPositionVaries_ReturnsExpected(int? position, bool expected)
        {
            // Arrange
            var entry = new RankingEntry(Guid.NewGuid(), 1, 1, 1, true, PerformanceStatus.Active, true, position, 100, true, [], []);

            // Act
            var result = PodiumAppearance.IsOnPodium(entry);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public Task PodiumFilter_WhenFiveEntriesTiedFirst_KeepsAllFive() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive(choreographyCount: 5);
            var page = archive.Performances.OpenPage(1);
            foreach (var judge in page.Judges.Where(judge => judge.IsRequired))
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    archive.Performances.SaveScore(judge.ChoreographyId, judge.JudgeId, criterion.Id, 80);
                }
            }

            using var form = new RankingForm(archive.Races, archive.Registry, archive.Choreographies, archive.Races.GetRaces().Single().Key, "Prova");

            // Act
            ((ComboBox)form.Controls.Find("rankingViewSelector", true).Single()).SelectedIndex = 1;

            // Assert
            var grid = form.Controls.OfType<SplitContainer>().Single().Panel1.Controls.OfType<DataGridView>().Single();
            grid.Rows.Count.Should().Be(5);
            grid.Rows.Cast<DataGridViewRow>().Should().OnlyContain(row => row.Cells["Position"].Value as string == "1");
        });

        [Theory]
        [InlineData(0, 2)]
        [InlineData(1, 0)]
        public Task PodiumFilter_WhenEntriesUnranked_ShowsOnlyInFullView(int selection, int expectedCount) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = new RankingForm(archive.Races, archive.Registry, archive.Choreographies, archive.Races.GetRaces().Single().Key, "Prova");
            var selector = (ComboBox)form.Controls.Find("rankingViewSelector", true).Single();
            selector.SelectedIndex = 1 - selection;

            // Act
            selector.SelectedIndex = selection;

            // Assert
            form.Controls.OfType<SplitContainer>().Single().Panel1.Controls.OfType<DataGridView>().Single().Rows.Count.Should().Be(expectedCount);
        });
    }
}
