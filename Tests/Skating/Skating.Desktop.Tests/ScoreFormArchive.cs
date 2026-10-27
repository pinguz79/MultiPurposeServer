using Skating.Core;
using Skating.Desktop.Configuration;
using Skating.Desktop.Scoring;
using Skating.Persistence;

namespace Skating.Desktop.Tests
{
    internal sealed class ScoreFormArchive : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-scores-ui-").FullName;

        internal SqliteRegistryStore Registry { get; }
        internal SqliteChoreographyStore Choreographies { get; }
        internal SqlitePerformanceStore Performances { get; }
        internal SqliteRaceStore Races { get; }
        internal SqliteIntermediateAwardStore Awards { get; }
        internal SqliteAbsoluteAwardStore AbsoluteAwards { get; }
        internal ScorePreferencesStore Preferences { get; }
        internal List<string> Alerts { get; } = [];

        internal ScoreFormArchive(int judgeCount = 3, int choreographyCount = 2)
        {
            var path = Path.Combine(_directory, "test.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            Registry = new SqliteRegistryStore(path);
            Choreographies = new SqliteChoreographyStore(path);
            Performances = new SqlitePerformanceStore(path);
            Races = new SqliteRaceStore(path);
            Awards = new SqliteIntermediateAwardStore(path);
            AbsoluteAwards = new SqliteAbsoluteAwardStore(path);
            Preferences = new ScorePreferencesStore(Path.Combine(_directory, "scores.json"));
            var school = new School(Guid.NewGuid(), "Scuola");
            var discipline = new Discipline(Guid.NewGuid(), "Disciplina");
            var category = new Category(Guid.NewGuid(), "Categoria");
            var level = new Level(Guid.NewGuid(), "Open", false);
            var athlete = new Athlete(Guid.NewGuid(), "Anna", "Rossi");
            Registry.AddSchool(school);
            Registry.AddDiscipline(discipline);
            Registry.AddCategory(category);
            Registry.AddLevel(level);
            Registry.AddAthlete(athlete);
            for (var index = 0; index < judgeCount; index++)
            {
                Registry.AddJudge(new Judge(Guid.NewGuid(), $"Giudice {index}", index == 2));
            }

            for (var index = 0; index < choreographyCount; index++)
            {
                Choreographies.AddChoreography(new Choreography(Guid.NewGuid(), $"Prova {index}", school.Id, discipline.Id, category.Id, level.Id, CompositionKind.Solo, 120, [athlete.Id]));
            }

            Performances.Prepare(20);
            Performances.ConfirmBlocks();
            Performances.Start();
        }

        internal ScoreEntryForm CreateForm() => new(Performances, Registry, Choreographies, 1, Preferences, Alerts.Add);

        public void Dispose() => Directory.Delete(_directory, true);
    }
}
