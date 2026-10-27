using Skating.Core;

namespace Skating.Persistence
{
    /// <summary>Classifiche assolute in uno snapshot coerente delle fonti.</summary>
    public sealed class SqliteAbsoluteAwardStore(string databasePath) : IAbsoluteAwardStore
    {
        private readonly SkatingDatabase _database = new(databasePath);

        /// <inheritdoc />
        public AbsoluteAward GetAward(AbsoluteAwardKind kind, AwardAggregation? aggregation = null)
        {
            using var session = new PerformanceSession(_database, true);
            return Read(session, kind, aggregation);
        }

        /// <inheritdoc />
        public void Validate(AbsoluteAwardKind kind, AwardAggregation? aggregation = null)
        {
            using var session = new PerformanceSession(_database);
            if (!Read(session, kind, aggregation).CanValidate)
            {
                throw new InvalidOperationException("Convalidare tutte le fonti e risolvere le parità sul podio prima di convalidare l'assoluta.");
            }

            session.Execute("UPDATE AbsoluteAward SET IsValidated = 1\nWHERE Kind = $kind;", ("$kind", (int)kind));
            session.Commit();
        }

        /// <inheritdoc />
        public void Reopen(AbsoluteAwardKind kind)
        {
            RequireKind(kind);
            using var session = new PerformanceSession(_database);
            session.Execute("UPDATE AbsoluteAward SET IsValidated = 0\nWHERE Kind = $kind;", ("$kind", (int)kind));
            session.Commit();
        }

        private static AbsoluteAward Read(PerformanceSession session, AbsoluteAwardKind kind, AwardAggregation? aggregation)
        {
            RequireKind(kind);
            IReadOnlyList<IntermediateAward> intermediates = [];
            List<RaceRanking> races = [];
            List<RankingEntry> winners;
            if (kind == AbsoluteAwardKind.NonPro)
            {
                intermediates = SqliteIntermediateAwardStore.Read(session, aggregation ?? throw new InvalidOperationException("Configurare il criterio dei premi intermedi per consultare l'assoluta non PRO."));
                races.AddRange(intermediates.SelectMany(award => award.Sources));
                winners = [.. intermediates.SelectMany(award => award.Entries.Where(entry => entry.Position == 1))];
            }
            else
            {
                List<RaceKey> keys = [];
                using (var command = session.Command("""
                    SELECT race.DisciplineId, race.CategoryId, race.LevelId
                    FROM RaceConfiguration AS race
                        INNER JOIN Level AS level ON level.Id = race.LevelId
                    WHERE level.IsPro = 1;
                    """))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        keys.Add(new RaceKey(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), Guid.Parse(reader.GetString(2))));
                    }
                }

                races.AddRange(keys.Select(key => RankingReader.Read(session, key)));
                winners = [.. races.SelectMany(race => race.Entries.Where(entry => entry.Position == 1))];
            }

            var validated = session.Scalar("SELECT IsValidated\nFROM AbsoluteAward\nWHERE Kind = $kind;", ("$kind", (int)kind)) != 0;
            var sourcesReady = kind == AbsoluteAwardKind.Pro ? races.Count > 0 && races.All(race => race.IsClosed)
                : intermediates.Count > 0 && intermediates.All(award => award.IsValidated);
            // I totali e le esclusioni delle fonti non vengono ereditati: nuovo confronto sui voti originali.
            return new AbsoluteAward(kind, validated && sourcesReady, intermediates, races, RankingCalculator.CalculateEntries(winners));
        }

        private static void RequireKind(AbsoluteAwardKind kind)
        {
            if (!Enum.IsDefined(kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}
