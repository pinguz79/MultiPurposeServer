namespace Skating.Core
{
    /// <summary>Iscrizione autonoma con scuola, combinazione di gara e insieme immutabile di atleti.</summary>
    public sealed class Choreography
    {
        public Guid Id { get; }

        public string Name { get; }

        public Guid SchoolId { get; }

        public Guid DisciplineId { get; }

        public Guid CategoryId { get; }

        public Guid LevelId { get; }

        public CompositionKind Composition { get; }

        public int DurationSeconds { get; }

        public IReadOnlyList<Guid> AthleteIds { get; }

        public Choreography(Guid id, string name, Guid schoolId, Guid disciplineId, Guid categoryId, Guid levelId, CompositionKind composition, int durationSeconds, IEnumerable<Guid> athleteIds)
        {
            if (id == Guid.Empty || schoolId == Guid.Empty || disciplineId == Guid.Empty || categoryId == Guid.Empty || levelId == Guid.Empty)
            {
                throw new ArgumentException("Coreografia, scuola, disciplina, categoria e livello devono avere un identificativo valido.");
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(athleteIds);
            ArgumentOutOfRangeException.ThrowIfNegative(durationSeconds);
            if (!Enum.IsDefined(composition))
            {
                throw new ArgumentOutOfRangeException(nameof(composition));
            }

            var members = athleteIds.ToArray();
            if (members.Any(member => member == Guid.Empty) || members.Distinct().Count() != members.Length)
            {
                throw new ArgumentException("Gli atleti devono essere validi e non ripetuti.", nameof(athleteIds));
            }

            if (composition == CompositionKind.Solo && members.Length != 1 || composition == CompositionKind.Duo && members.Length != 2 || composition == CompositionKind.Group && members.Length < 3)
            {
                throw new ArgumentException("Selezionare un atleta per Assolo, due per Duo, almeno tre per Gruppo.", nameof(athleteIds));
            }

            Id = id;
            Name = name.Trim();
            SchoolId = schoolId;
            DisciplineId = disciplineId;
            CategoryId = categoryId;
            LevelId = levelId;
            Composition = composition;
            DurationSeconds = durationSeconds;
            AthleteIds = Array.AsReadOnly(members);
        }
    }
}
