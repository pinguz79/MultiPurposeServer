using FluentAssertions;

using Skating.Desktop.Configuration;
using Skating.Desktop.Scoring;

namespace Skating.Desktop.Tests
{
    public sealed class ScoreSheetLayoutTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Render_WhenFiveJudges_DisplaysGroupedHeadersAndRowActions(bool colored) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive(5, 5);
            var page = archive.Performances.OpenPage(1);
            var entries = archive.Performances.GetSchedule().Performances;
            foreach (var performance in entries.Take(2))
            {
                foreach (var judge in page.Judges.Where(judge => judge.ChoreographyId == performance.ChoreographyId && judge.IsRequired))
                {
                    foreach (var criterion in archive.Registry.GetCriteria())
                    {
                        archive.Performances.SaveScore(performance.ChoreographyId, judge.JudgeId, criterion.Id, 78);
                    }
                }
            }

            archive.Performances.ConfirmComplete(1, entries[0].ChoreographyId);
            archive.Preferences.Save(new ScorePreferences(false, colored));
            using var form = archive.CreateForm();
            form.ClientSize = new Size(1600, 900);
            // Finestra trasparente fuori schermo: layout WinForms effettivo, senza UI visibile all'operatore.
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Show();
            form.PerformLayout();
            using var bitmap = new Bitmap(form.Width, form.Height);

            // Act
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));

            // Assert
            var grid = form.Controls.OfType<ScoreGrid>().Single();
            grid.JudgeNames.Should().HaveCount(5);
            grid.RowVisuals.Should().HaveCount(5);
            grid.ColorDiscipline.Should().Be(colored);
            grid.Columns["Action"].Should().NotBeNull();
            grid.Rows[0].Cells["Action"].Value.Should().Be("Riapri");
            grid.Rows[1].Cells["Status"].Value.Should().Be("Da confermare");
            grid.Rows[2].Cells["Total"].Value.Should().Be("—");
            // Esportazione diagnostica facoltativa per la verifica visuale, non usata dalla suite ordinaria.
            var previewDirectory = Environment.GetEnvironmentVariable("SKATING_PREVIEW_DIRECTORY");
            if (!string.IsNullOrWhiteSpace(previewDirectory))
            {
                Directory.CreateDirectory(previewDirectory);
                bitmap.Save(Path.Combine(previewDirectory, colored ? "scheda-sfondo.png" : "scheda-etichette.png"));
            }
        });

    }
}
