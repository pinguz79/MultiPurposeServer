using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Rankings;

namespace Skating.Desktop.Tests
{
    public sealed class IntermediateAwardsFormTests
    {
        [Theory]
        [InlineData(AwardAggregation.Discipline)]
        [InlineData(AwardAggregation.Category)]
        public Task Render_WhenWinnerReady_EnablesValidation(AwardAggregation mode) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive(choreographyCount: 1);
            archive.Awards.Configure(mode);
            var page = archive.Performances.OpenPage(1);
            foreach (var judge in page.Judges.Where(judge => judge.IsRequired))
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    archive.Performances.SaveScore(judge.ChoreographyId, judge.JudgeId, criterion.Id, 80);
                }
            }

            archive.Performances.ConfirmComplete(1);
            archive.Races.Close(archive.Races.GetRaces().Single().Key);
            using var form = new IntermediateAwardsForm(archive.Awards, archive.Registry, archive.Choreographies, mode, _ => { });
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Show();
            using var bitmap = new Bitmap(form.Width, form.Height);

            // Act
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));

            // Assert
            var grid = form.Controls.OfType<DataGridView>().Single();
            grid.Rows.Count.Should().Be(1);
            grid.Rows[0].Cells[0].Value.Should().Be("1");
            var validate = form.Controls.OfType<FlowLayoutPanel>().SelectMany(panel => panel.Controls.OfType<Button>()).Single(button => button.Text == "Convalida premio");
            validate.Enabled.Should().BeTrue();
            var directory = Environment.GetEnvironmentVariable("SKATING_PREVIEW_DIRECTORY");
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
                bitmap.Save(Path.Combine(directory, $"premio-{mode}.png"));
            }
        });

        [Fact]
        public Task IntermediateAwardsForm_WhenSourcesOpen_DisablesValidation() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            archive.Awards.Configure(AwardAggregation.Discipline);

            // Act
            using var form = new IntermediateAwardsForm(archive.Awards, archive.Registry, archive.Choreographies, AwardAggregation.Discipline, _ => { });

            // Assert
            var validate = form.Controls.OfType<FlowLayoutPanel>().SelectMany(panel => panel.Controls.OfType<Button>()).Single(button => button.Text == "Convalida premio");
            validate.Enabled.Should().BeFalse();
        });
    }
}
