using System.Reflection;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Configuration;
using Skating.Desktop.Manifestations;
using Skating.Desktop.Shell;
using Skating.Persistence;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class RecentManifestationMenuTests : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-recent-menu-").FullName;

        [Fact]
        public Task RecentItemClick_WhenArchiveExists_ReopensAndMovesToTop() => WinFormsTest.Run(() =>
        {
            // Arrange
            var recent = new RecentManifestationStore(Path.Combine(_directory, "recent.json"));
            var first = Path.Combine(_directory, "first.skating");
            var second = Path.Combine(_directory, "second.skating");
            new SqliteManifestationStore(first).Create(new Manifestation(Guid.NewGuid(), "Prima"));
            new SqliteManifestationStore(second).Create(new Manifestation(Guid.NewGuid(), "Seconda"));
            recent.Remember("Prima", first);
            recent.Remember("Seconda", second);
            using var form = new MainForm(path => new SqliteManifestationStore(path), path => new SqliteRegistryStore(path),
                path => new SqliteChoreographyStore(path), path => new SqliteRaceStore(path), recent, path => new SqlitePerformanceStore(path), new ScorePreferencesStore(Path.Combine(_directory, "scores.json")));
            var menuField = typeof(MainForm).GetField("manifestationMenuItem", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            var menu = menuField.GetValue(form) as ToolStripMenuItem ?? throw new InvalidOperationException();
            var item = menu.DropDownItems.OfType<ToolStripMenuItem>().Single(entry => entry.Tag as string == first);

            // Act
            item.PerformClick();

            // Assert
            form.Text.Should().Be("Skating System - Prima");
            recent.Load().Select(entry => entry.Path).Should().Equal(first, second);
            menu.DropDownItems.OfType<ToolStripMenuItem>().First(entry => entry.Tag is string).Tag.Should().Be(first);
        });

        public void Dispose() => Directory.Delete(_directory, true);
    }
}
