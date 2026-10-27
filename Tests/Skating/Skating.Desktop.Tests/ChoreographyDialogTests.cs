using System.Reflection;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Choreographies;
using Skating.Persistence;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class ChoreographyDialogTests : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-choreography-ui-").FullName;

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task SaveButtonClick_WhenValid_PersistsSelectedCatalogsAndMembers(bool editing) => WinFormsTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(_directory, "demo.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            var registry = new SqliteRegistryStore(path);
            var school = new School(Guid.NewGuid(), "Scuola");
            var discipline = new Discipline(Guid.NewGuid(), "Caraibico");
            var category = new Category(Guid.NewGuid(), "Senior");
            var level = new Level(Guid.NewGuid(), "Open", false);
            var athlete = new Athlete(Guid.NewGuid(), "Anna", "Rossi");
            registry.AddSchool(school);
            registry.AddDiscipline(discipline);
            registry.AddCategory(category);
            registry.AddLevel(level);
            registry.AddAthlete(athlete);
            var store = new SqliteChoreographyStore(path);
            Choreography? original = editing ? new Choreography(Guid.NewGuid(), "Originale", school.Id, discipline.Id, category.Id, level.Id, CompositionKind.Solo, 61, [athlete.Id]) : null;
            if (original is not null)
            {
                store.AddChoreography(original);
            }

            using var dialog = new ChoreographyDialog(registry, store, original);
            dialog.BindingContext = new BindingContext();
            Invoke(dialog, "ChoreographyDialogShown");
            if (!editing)
            {
                foreach (var name in new[] { "schoolCombo", "disciplineCombo", "categoryCombo", "levelCombo" })
                {
                    dialog.Controls.Find(name, true).OfType<ComboBox>().Single().SelectedIndex = 0;
                }

                dialog.Controls.Find("compositionCombo", true).OfType<ComboBox>().Single().SelectedItem = CompositionKind.Solo;
                dialog.Controls.Find("athletesList", true).OfType<CheckedListBox>().Single().SetItemChecked(0, true);
            }

            dialog.Controls.Find("nameTextBox", true).Single().Text = "Nome corretto";
            dialog.Controls.Find("minutesInput", true).OfType<NumericUpDown>().Single().Value = 3;
            dialog.Controls.Find("secondsInput", true).OfType<NumericUpDown>().Single().Value = 12;
            dialog.Controls.Find("athleteFilterTextBox", true).Single().Text = "nessuna corrispondenza";

            // Act
            Invoke(dialog, "SaveButtonClick");

            // Assert
            var saved = store.GetChoreographies().Should().ContainSingle().Subject;
            saved.Name.Should().Be("Nome corretto");
            saved.AthleteIds.Should().Equal(athlete.Id);
            saved.SchoolId.Should().Be(school.Id);
            saved.DurationSeconds.Should().Be(192);
            dialog.DialogResult.Should().Be(DialogResult.OK);
            if (original is not null)
            {
                saved.Id.Should().Be(original.Id);
            }
        });

        [Fact]
        public Task ChoreographyDialogShown_WhenCatalogsMissing_DisablesSave() => WinFormsTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(_directory, "demo.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            using var dialog = new ChoreographyDialog(new SqliteRegistryStore(path), new SqliteChoreographyStore(path));
            dialog.BindingContext = new BindingContext();

            // Act
            Invoke(dialog, "ChoreographyDialogShown");

            // Assert
            dialog.Controls.Find("saveButton", true).Single().Enabled.Should().BeFalse();
            dialog.Controls.Find("hintLabel", true).Single().Text.Should().Contain("Anagrafiche");
        });

        private static void Invoke(Form dialog, string method)
        {
            var handler = dialog.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            handler.Invoke(dialog, [null, EventArgs.Empty]);
        }

        public void Dispose() => Directory.Delete(_directory, true);
    }
}
