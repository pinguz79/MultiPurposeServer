using System.Reflection;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Configuration;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class ManifestationAwardsDialogTests
    {
        [Fact]
        public Task Save_WhenNoValidatedAwards_ChangesModeWithoutPromptOrRestart() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            archive.Awards.Configure(AwardAggregation.Discipline);
            var prompted = false;
            using var dialog = new ManifestationAwardsDialog(archive.Awards, _ => { prompted = true; return false; }, archive.Alerts.Add);
            dialog.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<ComboBox>().Single().SelectedIndex = 1;
            var save = typeof(ManifestationAwardsDialog).GetMethod("Save", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            var preview = Environment.GetEnvironmentVariable("SKATING_PREVIEW_DIRECTORY");
            if (!string.IsNullOrWhiteSpace(preview))
            {
                Directory.CreateDirectory(preview);
                dialog.ShowInTaskbar = false;
                dialog.Opacity = 0;
                dialog.StartPosition = FormStartPosition.Manual;
                dialog.Location = new Point(-30000, -30000);
                dialog.Show();
                using var bitmap = new Bitmap(dialog.Width, dialog.Height);
                dialog.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                bitmap.Save(Path.Combine(preview, "premi-manifestazione.png"));
            }

            // Act
            save.Invoke(dialog, null);

            // Assert
            prompted.Should().BeFalse();
            dialog.DialogResult.Should().Be(DialogResult.OK);
            archive.Awards.GetAggregation().Should().Be(AwardAggregation.Category);
            archive.Alerts.Should().BeEmpty();
        });

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Save_WhenValidated_ChangesOnlyAfterConsent(bool consent) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive(choreographyCount: 1);
            archive.Awards.Configure(AwardAggregation.Discipline);
            var entry = archive.Choreographies.GetChoreographies()[0];
            archive.Performances.OpenPage(1);
            foreach (var judge in archive.Registry.GetJudges().Where(judge => !judge.IsAbsent))
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    archive.Performances.SaveScore(entry.Id, judge.Id, criterion.Id, 80);
                }
            }

            archive.Performances.ConfirmComplete(1);
            archive.Races.Close(archive.Races.GetRaces()[0].Key);
            archive.Awards.Validate(AwardAggregation.Discipline, entry.DisciplineId);
            List<string> prompts = [];
            using var dialog = new ManifestationAwardsDialog(archive.Awards, message => { prompts.Add(message); return consent; }, archive.Alerts.Add);
            dialog.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<ComboBox>().Single().SelectedIndex = 1;
            var save = typeof(ManifestationAwardsDialog).GetMethod("Save", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            save.Invoke(dialog, null);

            // Assert
            prompts.Should().ContainSingle();
            archive.Awards.GetAggregation().Should().Be(consent ? AwardAggregation.Category : AwardAggregation.Discipline);
            archive.Awards.HasValidatedAwards().Should().Be(!consent);
            dialog.DialogResult.Should().Be(consent ? DialogResult.OK : DialogResult.None);
            archive.Alerts.Should().BeEmpty();
        });
    }
}
