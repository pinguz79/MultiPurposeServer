using System.Reflection;

using FluentAssertions;

using Skating.Desktop.Scoring;

namespace Skating.Desktop.Tests
{
    public sealed class ScoreConfirmationNavigationTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Navigate_WhenLastVoteCompleted_TabsToEnabledConfirmation(bool nextPerformance) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            var page = archive.Performances.OpenPage(1);
            var performance = archive.Performances.GetSchedule().Performances[0];
            var judges = archive.Registry.GetJudges().Where(judge => page.Judges.Any(entry => entry.ChoreographyId == performance.ChoreographyId && entry.JudgeId == judge.Id && entry.IsRequired)).ToList();
            var criteria = archive.Registry.GetCriteria();
            foreach (var judge in judges)
            {
                foreach (var criterion in criteria)
                {
                    if (judge != judges[^1] || criterion != criteria[^1])
                    {
                        archive.Performances.SaveScore(performance.ChoreographyId, judge.Id, criterion.Id, 80);
                    }
                }
            }

            using var form = archive.CreateForm();
            var grid = form.Controls.OfType<ScoreGrid>().Single();
            grid.EnterNextPerformance = nextPerformance;
            var lastColumn = grid.Columns.Cast<DataGridViewColumn>().Single(column => column.Tag is ScoreColumn score && score.JudgeId == judges[^1].Id && score.CriterionId == criteria[^1].Id);
            grid.Rows[0].Cells["Action"].Tag.Should().Be(false);
            grid.CurrentCell = grid.Rows[0].Cells[lastColumn.Index];
            grid.BeginEdit(true);
            var editor = grid.EditingControl as TextBox ?? throw new InvalidOperationException();
            editor.Text = "8";
            var navigate = typeof(ScoreGrid).GetMethod("Navigate", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            navigate.Invoke(grid, [Keys.Tab]);

            // Assert
            grid.Rows[0].Cells["Action"].Tag.Should().Be(true);
            grid.CurrentCell.Should().BeSameAs(grid.Rows[0].Cells["Action"]);
            grid.IsCurrentCellInEditMode.Should().BeFalse();
        });

        [Theory]
        [InlineData(Keys.Enter, true, 1)]
        [InlineData(Keys.Space, true, 1)]
        [InlineData(Keys.Enter, false, 0)]
        [InlineData(Keys.Space, false, 0)]
        public Task Navigate_WhenActionFocused_ClicksOnlyEnabledButton(Keys key, bool enabled, int expectedClicks) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var grid = new ScoreGrid { AllowUserToAddRows = false };
            grid.Columns.Add(new DataGridViewButtonColumn { Name = "Action", ReadOnly = true });
            grid.Rows.Add("Conferma");
            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.CurrentCell.Tag = enabled;
            var clicks = 0;
            grid.CellClick += (_, _) => clicks++;
            var navigate = typeof(ScoreGrid).GetMethod("Navigate", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            navigate.Invoke(grid, [key]);

            // Assert
            clicks.Should().Be(expectedClicks);
        });
    }
}
