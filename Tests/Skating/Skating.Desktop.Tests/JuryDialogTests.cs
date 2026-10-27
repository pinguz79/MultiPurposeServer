using System.Reflection;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Races;
using Skating.Persistence;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class JuryDialogTests : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-jury-ui-").FullName;

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Save_WhenChoosingJury_PersistsCustomOrDefault(bool useDefault) => WinFormsTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(_directory, "demo.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            var registry = new SqliteRegistryStore(path);
            var school = new School(Guid.NewGuid(), "Scuola");
            var discipline = new Discipline(Guid.NewGuid(), "Disciplina");
            var category = new Category(Guid.NewGuid(), "Categoria");
            var level = new Level(Guid.NewGuid(), "Open", false);
            var athlete = new Athlete(Guid.NewGuid(), "Anna", "Rossi");
            registry.AddSchool(school);
            registry.AddDiscipline(discipline);
            registry.AddCategory(category);
            registry.AddLevel(level);
            registry.AddAthlete(athlete);
            for (var index = 0; index < 4; index++)
            {
                registry.AddJudge(new Judge(Guid.NewGuid(), $"Giudice {index}"));
            }

            new SqliteChoreographyStore(path).AddChoreography(new Choreography(Guid.NewGuid(), "Prova", school.Id, discipline.Id, category.Id, level.Id, CompositionKind.Solo, 0, [athlete.Id]));
            var store = new SqliteRaceStore(path);
            var race = store.GetRaces().Single();
            using var dialog = new JuryDialog(registry, store, race, "Prova", ["Coreografia"]);
            var list = dialog.Controls.Find("judgesList", true).OfType<CheckedListBox>().Single();
            list.SetItemChecked(0, false);
            var method = typeof(JuryDialog).GetMethod(useDefault ? "DefaultButtonClick" : "SaveButtonClick", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            method.Invoke(dialog, [null, EventArgs.Empty]);

            // Assert
            store.GetRaces().Single().JudgeIds.Should().HaveCount(useDefault ? 4 : 3);
            store.GetRaces().Single().HasCustomJury.Should().Be(!useDefault);
            dialog.DialogResult.Should().Be(DialogResult.OK);
        });

        public void Dispose() => Directory.Delete(_directory, true);
    }
}
