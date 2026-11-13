using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Races;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class RaceColorsFormTests
    {
        [Fact]
        public Task ApplyColor_WhenSelected_SavesAndUpdatesLegend() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = new RaceColorsForm(archive.Races, archive.Registry);
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            form.Show();
            var toolbar = form.Controls.OfType<FlowLayoutPanel>().Single();
            toolbar.Controls.OfType<ComboBox>().Single().SelectedItem = "Rosa";

            // Act
            toolbar.Controls.OfType<Button>().Single().PerformClick();

            // Assert
            archive.Races.GetColors().Values.Should().ContainSingle().Which.Should().Be("Rosa");
            form.Controls.OfType<DataGridView>().Single().Rows[0].Cells[1].Value.Should().Be("Rosa");
            var directory = Environment.GetEnvironmentVariable("SKATING_PREVIEW_DIRECTORY");
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
                using var bitmap = new Bitmap(form.Width, form.Height);
                form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                bitmap.Save(Path.Combine(directory, "colori-legenda.png"));
            }
        });

        [Fact]
        public void Palette_WhenUsedWithBlackText_HasUniqueColorsAndHighContrast()
        {
            // Arrange
            var palette = RaceColor.Palette;

            // Act
            var contrasts = palette.Select(color =>
            {
                var red = Linear((color.Rgb >> 16) & 255);
                var green = Linear((color.Rgb >> 8) & 255);
                var blue = Linear(color.Rgb & 255);
                return (0.2126 * red + 0.7152 * green + 0.0722 * blue + 0.05) / 0.05;
            }).ToArray();

            // Assert
            contrasts.Should().OnlyContain(contrast => contrast >= 7);
            palette.Select(color => color.Name).Should().OnlyHaveUniqueItems();
            palette.Select(color => color.Rgb).Should().OnlyHaveUniqueItems();
        }

        private static double Linear(int component)
        {
            var value = component / 255d;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
    }
}
