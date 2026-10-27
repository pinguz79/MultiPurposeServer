using System.Reflection;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Choreographies;
using Skating.Desktop.Common;
using Skating.Persistence;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class AthleteFilterTests : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-filter-ui-").FullName;
        private readonly SqliteRegistryStore _registry;
        private readonly SqliteChoreographyStore _store;

        public AthleteFilterTests()
        {
            var path = Path.Combine(_directory, "demo.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            _registry = new SqliteRegistryStore(path);
            _store = new SqliteChoreographyStore(path);
            _registry.AddAthlete(new Athlete(Guid.NewGuid(), "Anna Maria", "Rossi"));
            _registry.AddAthlete(new Athlete(Guid.NewGuid(), "Luca", "Bianchi"));
        }

        [Theory]
        [InlineData("ANNA", 1)]
        [InlineData("ross", 1)]
        [InlineData("rossi anna", 1)]
        [InlineData("  maria   ROS  ", 1)]
        [InlineData("Anna Bianchi", 0)]
        [InlineData("coreografie", 0)]
        [InlineData(" ", 2)]
        public Task Filter_WhenTextChanges_MatchesOnlyNameAndSurname(string text, int count) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var dialog = Open();

            // Act
            dialog.Controls.Find("athleteFilterTextBox", true).Single().Text = text;

            // Assert
            dialog.Controls.Find("athletesList", true).OfType<CheckedListBox>().Single().Items.Count.Should().Be(count);
        });

        [Fact]
        public Task Filter_WhenSelectionIsHiddenAndRestored_PreservesChecks() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var dialog = Open();
            var list = dialog.Controls.Find("athletesList", true).OfType<CheckedListBox>().Single();
            var filter = dialog.Controls.Find("athleteFilterTextBox", true).Single();
            list.SetItemChecked(0, true);
            var selected = list.CheckedItems.OfType<NamedChoice>().Single().Id;
            filter.Text = "inesistente";

            // Act
            filter.Text = "";

            // Assert
            list.CheckedItems.OfType<NamedChoice>().Should().ContainSingle().Which.Id.Should().Be(selected);
        });

        [Fact]
        public Task ReloadAthletes_WhenNewAthleteWasCreated_PreservesFilterSelectionAndEditor() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var dialog = Open();
            var list = dialog.Controls.Find("athletesList", true).OfType<CheckedListBox>().Single();
            list.SetItemChecked(0, true);
            var selected = list.CheckedItems.OfType<NamedChoice>().Single().Id;
            dialog.Controls.Find("nameTextBox", true).Single().Text = "Coreografia in corso";
            dialog.Controls.Find("athleteFilterTextBox", true).Single().Text = "Verdi";
            var added = new Athlete(Guid.NewGuid(), "Giulia", "Verdi");
            _registry.AddAthlete(added);
            var reload = typeof(ChoreographyDialog).GetMethod("ReloadAthletes", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            reload.Invoke(dialog, null);

            // Assert
            list.Items.OfType<NamedChoice>().Should().ContainSingle().Which.Id.Should().Be(added.Id);
            dialog.Controls.Find("nameTextBox", true).Single().Text.Should().Be("Coreografia in corso");
            dialog.Controls.Find("athleteFilterTextBox", true).Single().Text.Should().Be("Verdi");
            dialog.Controls.Find("athleteCountLabel", true).Single().Text.Should().Contain("1 selezionati");
            dialog.Controls.Find("athleteFilterTextBox", true).Single().Text = "";
            list.CheckedItems.OfType<NamedChoice>().Should().ContainSingle().Which.Id.Should().Be(selected);
        });

        private ChoreographyDialog Open()
        {
            var dialog = new ChoreographyDialog(_registry, _store) { BindingContext = new BindingContext() };
            var shown = typeof(ChoreographyDialog).GetMethod("ChoreographyDialogShown", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            shown.Invoke(dialog, [null, EventArgs.Empty]);
            return dialog;
        }

        public void Dispose() => Directory.Delete(_directory, true);
    }
}
