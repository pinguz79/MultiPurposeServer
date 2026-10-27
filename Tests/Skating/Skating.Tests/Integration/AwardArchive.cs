using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    internal sealed class AwardArchive : IDisposable
    {
        private readonly ChoreographyArchive _archive = new();

        internal string DatabasePath => _archive.DatabasePath;
        internal SqliteRegistryStore Registry => _archive.Registry;
        internal SqliteChoreographyStore Choreographies => _archive.Store;
        internal SqliteIntermediateAwardStore Awards { get; }
        internal SqliteRaceStore Races { get; }
        internal SqlitePerformanceStore Performances { get; }
        internal Guid DisciplineId => _archive.DisciplineId;
        internal Guid CategoryId => _archive.CategoryId;
        internal Guid WinnerId { get; }
        internal Guid ProId { get; }

        internal AwardArchive(AwardAggregation mode = AwardAggregation.Discipline, bool close = true)
        {
            Awards = new SqliteIntermediateAwardStore(DatabasePath);
            Races = new SqliteRaceStore(DatabasePath);
            Performances = new SqlitePerformanceStore(DatabasePath);
            var category = new Category(Guid.NewGuid(), "Kids");
            var discipline = new Discipline(Guid.NewGuid(), "Moderno");
            var pro = new Level(Guid.NewGuid(), "Pro", true);
            Registry.AddCategory(category);
            Registry.AddDiscipline(discipline);
            Registry.AddLevel(pro);
            for (var index = 0; index < 3; index++)
            {
                Registry.AddJudge(new Judge(Guid.NewGuid(), $"Giudice {index}"));
            }

            var winner = _archive.Entry("Vincitore A");
            WinnerId = winner.Id;
            Choreographies.AddChoreography(winner);
            Choreographies.AddChoreography(_archive.Entry("Secondo A"));
            Choreographies.AddChoreography(new Choreography(Guid.NewGuid(), "Vincitore B", winner.SchoolId, winner.DisciplineId, category.Id, winner.LevelId, winner.Composition, 120, winner.AthleteIds));
            Choreographies.AddChoreography(_archive.Entry("Vincitore C", discipline: discipline.Id));
            ProId = Guid.NewGuid();
            Choreographies.AddChoreography(new Choreography(ProId, "Vincitore PRO", winner.SchoolId, winner.DisciplineId, winner.CategoryId, pro.Id, winner.Composition, 120, winner.AthleteIds));
            Awards.Configure(mode);
            Performances.Prepare(20);
            Performances.ConfirmBlocks();
            Performances.Start();
            Performances.OpenPage(1);
            foreach (var entry in Choreographies.GetChoreographies())
            {
                var score = entry.Name switch { "Vincitore A" => 80, "Vincitore B" => 75, "Vincitore PRO" => 95, _ => 70 };
                foreach (var judge in Registry.GetJudges())
                {
                    foreach (var criterion in Registry.GetCriteria())
                    {
                        Performances.SaveScore(entry.Id, judge.Id, criterion.Id, score);
                    }
                }
            }

            Performances.ConfirmComplete(1);
            if (close)
            {
                foreach (var race in Races.GetRaces())
                {
                    Races.Close(race.Key);
                }
            }
        }

        internal RaceKey RaceOf(Guid id) => Races.GetRaces().Single(race => race.ChoreographyIds.Contains(id)).Key;

        public void Dispose() => _archive.Dispose();
    }
}
