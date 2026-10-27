using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    internal sealed class ChoreographyArchive : IDisposable
    {
        private readonly RegistryArchive _archive = new();

        internal string DatabasePath => _archive.DatabasePath;

        internal SqliteRegistryStore Registry { get; }

        internal SqliteChoreographyStore Store { get; }

        internal Guid SchoolId { get; } = Guid.NewGuid();

        internal Guid DisciplineId { get; } = Guid.NewGuid();

        internal Guid CategoryId { get; } = Guid.NewGuid();

        internal Guid LevelId { get; } = Guid.NewGuid();

        internal Guid[] Athletes { get; } = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

        internal ChoreographyArchive()
        {
            _archive.Create();
            Registry = new SqliteRegistryStore(DatabasePath);
            Store = new SqliteChoreographyStore(DatabasePath);
            Registry.AddSchool(new School(SchoolId, "Scuola"));
            Registry.AddDiscipline(new Discipline(DisciplineId, "Caraibico"));
            Registry.AddCategory(new Category(CategoryId, "Senior"));
            Registry.AddLevel(new Level(LevelId, "Open", false));
            foreach (var id in Athletes)
            {
                Registry.AddAthlete(new Athlete(id, "Nome", "Cognome"));
            }
        }

        internal Choreography Entry(string name = "Coreografia", Guid? id = null, IEnumerable<Guid>? athletes = null, Guid? school = null, Guid? discipline = null) => new(id ?? Guid.NewGuid(), name, school ?? SchoolId, discipline ?? DisciplineId, CategoryId, LevelId, CompositionKind.Duo, 125, athletes ?? Athletes.Take(2));

        public void Dispose() => _archive.Dispose();
    }
}
