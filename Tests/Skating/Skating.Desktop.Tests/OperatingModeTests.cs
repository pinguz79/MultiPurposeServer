using System.Reflection;

using FluentAssertions;

using Skating.Desktop.Choreographies;
using Skating.Desktop.Common;
using Skating.Desktop.Configuration;
using Skating.Desktop.Manifestations;
using Skating.Desktop.Races;
using Skating.Desktop.Rankings;
using Skating.Desktop.Scheduling;
using Skating.Desktop.Scoring;
using Skating.Desktop.Shell;
using Skating.Persistence;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class OperatingModeTests
    {
        [Fact]
        public Task ModeMenu_WhenSwitched_ChangesAppearanceAndKeepsSingleRankingsMenu() => WinFormsTest.Run(() =>
        {
            // Arrange
            var directory = Directory.CreateTempSubdirectory("skating-mode-");
            try
            {
                using var form = new MainForm(path => new SqliteManifestationStore(path), path => new SqliteRegistryStore(path), path => new SqliteChoreographyStore(path),
                    path => new SqliteRaceStore(path), new RecentManifestationStore(Path.Combine(directory.FullName, "recent.json")), path => new SqlitePerformanceStore(path),
                    new ScorePreferencesStore(Path.Combine(directory.FullName, "scores.json")));
                var menu = form.Controls.OfType<MenuStrip>().Single();
                var modeMenu = menu.Items.OfType<ToolStripMenuItem>().Single(item => item.Text == "Modalità");
                var initialColor = form.BackColor;
                var initiallyVisible = menu.Items.OfType<ToolStripMenuItem>().Single(item => item.Text == "Classifiche").Available;

                // Act
                modeMenu.DropDownItems.OfType<ToolStripMenuItem>().Single(item => item.Text == "Gara").PerformClick();

                // Assert
                form.BackColor.Should().NotBe(initialColor);
                form.Controls.Find("operatingModeLabel", false).Single().Text.Should().Be("Modalità Gara");
                modeMenu.DropDownItems.OfType<ToolStripMenuItem>().Single(item => item.Text == "Gara").Checked.Should().BeTrue();
                menu.Items.OfType<ToolStripMenuItem>().Single(item => item.Name == "registryMenuItem").Available.Should().BeFalse();
                var rankings = menu.Items.OfType<ToolStripMenuItem>().Single(item => item.Text == "Classifiche");
                rankings.DropDownItems.Count.Should().Be(3);
                initiallyVisible.Should().BeFalse();
                rankings.Available.Should().BeTrue();
                rankings.Enabled.Should().BeFalse();
                menu.Items.OfType<ToolStripMenuItem>().Should().NotContain(item => item.Text == "Premi intermedi..." || item.Text == "Assolute");
            }
            finally
            {
                directory.Delete(true);
            }
        });

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task RaceDoubleClick_WhenModeChanges_OpensJuryOrRanking(bool competition) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = new RacesForm(archive.Registry, archive.Choreographies, archive.Races, mode: competition ? OperatingMode.Competition : OperatingMode.Registry);
            ShowHidden(form);
            Type? openedType = null;
            form.BeginInvoke(() =>
            {
                var child = form.OwnedForms.Single();
                openedType = child.GetType();
                child.Close();
            });

            // Act
            InvokeDoubleClick(form, "EntriesGridCellDoubleClick");

            // Assert
            openedType.Should().Be(competition ? typeof(RankingForm) : typeof(JuryDialog));
            archive.Performances.GetSchedule().IsStarted.Should().BeTrue();
        });

        [Fact]
        public Task ChoreographyDoubleClick_WhenCompetition_RequestsSelectedScores() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            Guid? requested = null;
            using var form = new ChoreographyForm(archive.Registry, archive.Choreographies, OperatingMode.Competition, id => requested = id);
            ShowHidden(form);
            var grid = form.Controls.OfType<DataGridView>().Single();
            var expected = (grid.Rows[0].DataBoundItem as ChoreographyRow ?? throw new InvalidOperationException()).Id;

            // Act
            InvokeDoubleClick(form, "EntriesGridCellDoubleClick");

            // Assert
            requested.Should().Be(expected);
        });

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task ScheduleDoubleClick_WhenModeChanges_OpensChoreographyOrScores(bool competition) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = new ScheduleForm(archive.Performances, archive.Registry, archive.Choreographies, archive.Preferences, archive.Races,
                competition ? OperatingMode.Competition : OperatingMode.Registry);
            ShowHidden(form);
            Type? openedType = null;
            form.BeginInvoke(() =>
            {
                var child = form.OwnedForms.Single();
                openedType = child.GetType();
                child.Close();
            });
            var grid = form.Controls.OfType<DataGridView>().Single();
            var handler = typeof(DataGridView).GetMethod("OnCellDoubleClick", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            handler.Invoke(grid, [new DataGridViewCellEventArgs(0, 0)]);

            // Assert
            openedType.Should().Be(competition ? typeof(ScoreEntryForm) : typeof(ChoreographyDialog));
        });

        private static void ShowHidden(Form form)
        {
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Show();
            Application.DoEvents();
        }

        private static void InvokeDoubleClick(Form form, string name)
        {
            var handler = form.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            handler.Invoke(form, [form, new DataGridViewCellEventArgs(0, 0)]);
        }
    }
}
