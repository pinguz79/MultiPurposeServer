using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Rankings;
using Skating.Desktop.Scoring;
using Skating.Persistence;

namespace Skating.Desktop.Tests
{
    public sealed class RankingFormTests
    {
        [Fact]
        public Task RaceSelector_WhenChanged_DisplaysSelectedRace() => WinFormsTest.Run(() =>
        {
            // Arrange
            var directory = Directory.CreateTempSubdirectory("skating-rankings-");
            try
            {
                var path = Path.Combine(directory.FullName, "test.skating");
                new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
                var registry = new SqliteRegistryStore(path);
                var choreographies = new SqliteChoreographyStore(path);
                var races = new SqliteRaceStore(path);
                var school = new School(Guid.NewGuid(), "Scuola");
                var discipline = new Discipline(Guid.NewGuid(), "Salsa");
                var category = new Category(Guid.NewGuid(), "Senior");
                var level = new Level(Guid.NewGuid(), "Open", false);
                var secondLevel = new Level(Guid.NewGuid(), "Pro", true);
                var athlete = new Athlete(Guid.NewGuid(), "Anna", "Rossi");
                registry.AddSchool(school);
                registry.AddDiscipline(discipline);
                registry.AddCategory(category);
                registry.AddLevel(level);
                registry.AddLevel(secondLevel);
                registry.AddAthlete(athlete);
                choreographies.AddChoreography(new Choreography(Guid.NewGuid(), "Prima", school.Id, discipline.Id, category.Id, level.Id, CompositionKind.Solo, 120, [athlete.Id]));
                choreographies.AddChoreography(new Choreography(Guid.NewGuid(), "Seconda", school.Id, discipline.Id, category.Id, secondLevel.Id, CompositionKind.Solo, 120, [athlete.Id]));
                using var form = new RankingForm(races, registry, choreographies, new RaceKey(discipline.Id, category.Id, level.Id), "Prima", selectRace: true);
                var selector = (ComboBox)form.Controls.Find("raceSelector", true).Single();

                // Act
                selector.SelectedIndex = 1;

                // Assert
                var grid = form.Controls.OfType<SplitContainer>().Single().Panel1.Controls.OfType<DataGridView>().Single();
                grid.Rows.Count.Should().Be(1);
                grid.Rows[0].Cells["Name"].Value.Should().Be("Seconda");
                form.Text.Should().Contain("Pro");
            }
            finally
            {
                directory.Delete(true);
            }
        });

        [Fact]
        public Task RankingForm_WhenEmptyScores_DisplaysUnrankedRowsAndDisabledClosure() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            var key = archive.Races.GetRaces().Single().Key;

            // Act
            using var form = new RankingForm(archive.Races, archive.Registry, archive.Choreographies, key, "Prova");

            // Assert
            var grid = form.Controls.OfType<SplitContainer>().Single().Panel1.Controls.OfType<DataGridView>().Single();
            grid.Rows.Count.Should().Be(2);
            grid.Rows[0].Cells["Position"].Value.Should().Be("—");
            grid.Rows[0].Cells["Status"].Value.Should().Be("Senza voti");
            form.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<Button>().Single(button => button.Text == "Chiudi gara").Enabled.Should().BeFalse();
        });

        [Fact]
        public Task ScoreEntryForm_WhenExcluded_DisablesScoresAndConfirmation() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            archive.Races.SetStatus(archive.Choreographies.GetChoreographies()[0].Id, PerformanceStatus.Disqualified);

            // Act
            using var form = archive.CreateForm();

            // Assert
            var row = form.Controls.OfType<ScoreGrid>().Single().Rows[0];
            row.Cells["Status"].Value.Should().Be("Squalificata");
            row.Cells["Action"].Tag.Should().Be(false);
            row.Cells[2].ReadOnly.Should().BeTrue();
        });

        [Fact]
        public Task Render_WhenPodiumTie_DisplaysWarningAndScores() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            var page = archive.Performances.OpenPage(1);
            foreach (var judge in page.Judges.Where(judge => judge.IsRequired))
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    archive.Performances.SaveScore(judge.ChoreographyId, judge.JudgeId, criterion.Id, 80);
                }
            }

            archive.Performances.ConfirmComplete(1);
            using var form = new RankingForm(archive.Races, archive.Registry, archive.Choreographies, archive.Races.GetRaces().Single().Key, "Caraibico / Senior / Open");
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Show();
            using var bitmap = new Bitmap(form.Width, form.Height);

            // Act
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));

            // Assert
            var grid = form.Controls.OfType<SplitContainer>().Single().Panel1.Controls.OfType<DataGridView>().Single();
            grid.Rows[0].Cells["Ties"].Value.Should().Be("Parità sul podio");
            grid.Rows[1].Cells["Position"].Value.Should().Be("1");
            var previewDirectory = Environment.GetEnvironmentVariable("SKATING_PREVIEW_DIRECTORY");
            if (!string.IsNullOrWhiteSpace(previewDirectory))
            {
                Directory.CreateDirectory(previewDirectory);
                bitmap.Save(Path.Combine(previewDirectory, "classifica.png"));
            }
        });
    }
}
