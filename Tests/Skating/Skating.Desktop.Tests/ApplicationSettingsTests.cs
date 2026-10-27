using System.Text.Json;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Configuration;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class ApplicationSettingsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData(AwardAggregation.Discipline)]
        [InlineData(AwardAggregation.Category)]
        public void Save_WhenExistingJson_PreservesOtherSettingsAndActiveOptions(AwardAggregation? mode)
        {
            // Arrange
            var directory = Directory.CreateTempSubdirectory("skating-settings-");
            var path = Path.Combine(directory.FullName, "settings.json");
            try
            {
                File.WriteAllText(path, """{"IntermediateAwardAggregation":"Discipline","FutureSetting":{"Enabled":true}}""");
                var active = AwardOptions.Load(path);

                // Act
                new AwardOptions(mode).Save(path);

                // Assert
                AwardOptions.Load(path).IntermediateAwardAggregation.Should().Be(mode);
                active.IntermediateAwardAggregation.Should().Be(AwardAggregation.Discipline);
                using var json = JsonDocument.Parse(File.ReadAllText(path));
                json.RootElement.GetProperty("FutureSetting").GetProperty("Enabled").GetBoolean().Should().BeTrue();
                Directory.GetFiles(directory.FullName, "*.tmp").Should().BeEmpty();
            }
            finally
            {
                directory.Delete(true);
            }
        }

        [Fact]
        public void Save_WhenDestinationLocked_PreservesOriginalAndRemovesTemporary()
        {
            // Arrange
            var directory = Directory.CreateTempSubdirectory("skating-settings-");
            var path = Path.Combine(directory.FullName, "settings.json");
            try
            {
                File.WriteAllText(path, """{"IntermediateAwardAggregation":"Discipline"}""");
                var original = File.ReadAllText(path);
                using var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Act
                var action = () => new AwardOptions(AwardAggregation.Category).Save(path);

                // Assert
                action.Should().Throw<IOException>();
                File.ReadAllText(path).Should().Be(original);
                Directory.GetFiles(directory.FullName, "*.tmp").Should().BeEmpty();
            }
            finally
            {
                directory.Delete(true);
            }
        }

        [Fact]
        public void Save_WhenMissing_CreatesConfiguration()
        {
            // Arrange
            var directory = Directory.CreateTempSubdirectory("skating-settings-");
            var path = Path.Combine(directory.FullName, "settings.json");
            try
            {
                // Act
                new AwardOptions(AwardAggregation.Category).Save(path);

                // Assert
                AwardOptions.Load(path).IntermediateAwardAggregation.Should().Be(AwardAggregation.Category);
            }
            finally
            {
                directory.Delete(true);
            }
        }

        [Fact]
        public Task Save_WhenDialogEdited_ShowsRestartNoticeAndPersistsSelection() => WinFormsTest.Run(() =>
        {
            // Arrange
            var directory = Directory.CreateTempSubdirectory("skating-settings-ui-");
            var path = Path.Combine(directory.FullName, "settings.json");
            try
            {
                new AwardOptions(AwardAggregation.Discipline).Save(path);
                List<string> alerts = [];
                using var dialog = new ApplicationSettingsDialog(path, AwardAggregation.Discipline, alerts.Add);
                dialog.ShowInTaskbar = false;
                dialog.Opacity = 0;
                dialog.StartPosition = FormStartPosition.Manual;
                dialog.Location = new Point(-30000, -30000);
                dialog.Show();
                var layout = dialog.Controls.OfType<FlowLayoutPanel>().Single();
                layout.Controls.OfType<ComboBox>().Single().SelectedIndex = 2;
                var save = layout.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<Button>().Single(button => button.Text == "Salva");
                using var bitmap = new Bitmap(dialog.Width, dialog.Height);
                dialog.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                var preview = Environment.GetEnvironmentVariable("SKATING_PREVIEW_DIRECTORY");
                if (!string.IsNullOrWhiteSpace(preview))
                {
                    Directory.CreateDirectory(preview);
                    bitmap.Save(Path.Combine(preview, "impostazioni.png"));
                }

                // Act
                save.PerformClick();

                // Assert
                dialog.DialogResult.Should().Be(DialogResult.OK);
                alerts.Should().ContainSingle().Which.Should().Contain("Riavviare");
                AwardOptions.Load(path).IntermediateAwardAggregation.Should().Be(AwardAggregation.Category);
            }
            finally
            {
                directory.Delete(true);
            }
        });

        [Fact]
        public Task ApplicationSettingsDialog_WhenCancelled_DoesNotWrite() => WinFormsTest.Run(() =>
        {
            // Arrange
            var directory = Directory.CreateTempSubdirectory("skating-settings-ui-");
            var path = Path.Combine(directory.FullName, "settings.json");
            try
            {
                new AwardOptions(AwardAggregation.Discipline).Save(path);
                var original = File.ReadAllText(path);
                using var dialog = new ApplicationSettingsDialog(path, AwardAggregation.Discipline);
                dialog.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<ComboBox>().Single().SelectedIndex = 2;

                // Act
                dialog.Close();

                // Assert
                File.ReadAllText(path).Should().Be(original);
            }
            finally
            {
                directory.Delete(true);
            }
        });
    }
}
