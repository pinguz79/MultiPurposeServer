using Skating.Core;

namespace Skating.Persistence
{
    /// <summary>Premi intermedi ricalcolati nello stesso snapshot delle gare sorgenti.</summary>
    public sealed class SqliteIntermediateAwardStore(string databasePath) : IIntermediateAwardStore
    {
        private readonly SkatingDatabase _database = new(databasePath);

        /// <inheritdoc />
        public AwardAggregation? GetAggregation()
        {
            using var session = new PerformanceSession(_database, true);
            var value = session.Scalar("SELECT COALESCE(Aggregation, 0)\nFROM AwardSettings;");
            return value == 0 ? null : (AwardAggregation)value;
        }

        /// <inheritdoc />
        public void Configure(AwardAggregation aggregation)
        {
            RequireMode(aggregation);
            using var session = new PerformanceSession(_database);
            var current = session.Scalar("SELECT COALESCE(Aggregation, 0)\nFROM AwardSettings;");
            if (current == (int)aggregation)
            {
                return;
            }

            if (current != 0 && session.Scalar("SELECT IsStarted\nFROM EventState;") != 0)
            {
                throw new InvalidOperationException("La manifestazione è avviata con un diverso criterio premi. Ripristinare il parametro JSON originale e riavviare l'applicazione.");
            }

            session.Execute("UPDATE AwardSettings SET Aggregation = $mode;", ("$mode", (int)aggregation));
            session.Execute("UPDATE IntermediateAward SET IsValidated = 0;");
            session.Execute("UPDATE AbsoluteAward SET IsValidated = 0\nWHERE Kind = 0;");
            session.Commit();
        }

        /// <inheritdoc />
        public IReadOnlyList<IntermediateAward> GetAwards(AwardAggregation aggregation)
        {
            using var session = new PerformanceSession(_database, true);
            return Read(session, aggregation);
        }

        /// <inheritdoc />
        public void Validate(AwardAggregation aggregation, Guid groupId)
        {
            using var session = new PerformanceSession(_database);
            var award = Read(session, aggregation).SingleOrDefault(entry => entry.GroupId == groupId) ?? throw new InvalidOperationException("Il premio non esiste più.");
            if (!award.CanValidate)
            {
                throw new InvalidOperationException("Chiudere tutte le gare di origine e risolvere le parità sul podio prima di convalidare il premio.");
            }

            session.Execute("""
                INSERT INTO IntermediateAward (Aggregation, GroupId, IsValidated) VALUES ($mode, $group, 1)
                ON CONFLICT (Aggregation, GroupId) DO UPDATE SET IsValidated = 1;
                """, ("$mode", (int)aggregation), ("$group", groupId.ToString()));
            session.Commit();
        }

        /// <inheritdoc />
        public void Reopen(AwardAggregation aggregation, Guid groupId)
        {
            using var session = new PerformanceSession(_database);
            CheckConfiguration(session, aggregation);
            session.Execute("UPDATE IntermediateAward SET IsValidated = 0\nWHERE Aggregation = $mode AND GroupId = $group;", ("$mode", (int)aggregation), ("$group", groupId.ToString()));
            session.Execute("UPDATE AbsoluteAward SET IsValidated = 0\nWHERE Kind = 0;");
            session.Commit();
        }

        internal static IReadOnlyList<IntermediateAward> Read(PerformanceSession session, AwardAggregation aggregation)
        {
            CheckConfiguration(session, aggregation);
            List<(RaceKey Key, Guid GroupId, string Name)> sources = [];
            using (var command = session.Command("""
                SELECT race.DisciplineId, race.CategoryId, race.LevelId, discipline.Name, category.Name
                FROM RaceConfiguration AS race
                    INNER JOIN Level AS level ON level.Id = race.LevelId
                    INNER JOIN Discipline AS discipline ON discipline.Id = race.DisciplineId
                    INNER JOIN Category AS category ON category.Id = race.CategoryId
                WHERE level.IsPro = 0;
                """))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var key = new RaceKey(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), Guid.Parse(reader.GetString(2)));
                    sources.Add((key, aggregation == AwardAggregation.Discipline ? key.DisciplineId : key.CategoryId, reader.GetString(aggregation == AwardAggregation.Discipline ? 3 : 4)));
                }
            }

            List<IntermediateAward> awards = [];
            foreach (var group in sources.GroupBy(source => source.GroupId))
            {
                var races = group.Select(source => RankingReader.Read(session, source.Key)).ToList();
                // Si riparte dai voti originali: esclusioni e spareggi non sono bonus locali al premio.
                var winners = races.SelectMany(race => race.Entries.Where(entry => entry.Position == 1)).ToList();
                var validated = session.Scalar("SELECT IsValidated\nFROM IntermediateAward\nWHERE Aggregation = $mode AND GroupId = $group;",
                    ("$mode", (int)aggregation), ("$group", group.Key.ToString())) != 0;
                awards.Add(new IntermediateAward(group.Key, group.First().Name, aggregation, validated && races.All(race => race.IsClosed), races, RankingCalculator.CalculateEntries(winners)));
            }

            return [.. awards.OrderBy(award => award.Name, StringComparer.CurrentCultureIgnoreCase)];
        }

        private static void CheckConfiguration(PerformanceSession session, AwardAggregation aggregation)
        {
            RequireMode(aggregation);
            if (session.Scalar("SELECT COALESCE(Aggregation, 0)\nFROM AwardSettings;") != (int)aggregation)
            {
                throw new InvalidOperationException("Il criterio premi del JSON non coincide con quello dell'archivio. Verificare la configurazione prima di continuare.");
            }
        }

        private static void RequireMode(AwardAggregation aggregation)
        {
            if (!Enum.IsDefined(aggregation))
            {
                throw new ArgumentOutOfRangeException(nameof(aggregation));
            }
        }
    }
}
