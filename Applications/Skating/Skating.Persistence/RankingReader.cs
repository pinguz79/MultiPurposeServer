using Skating.Core;

namespace Skating.Persistence
{
    internal static class RankingReader
    {
        internal static (string, object)[] Arguments(RaceKey key) => [("$discipline", key.DisciplineId.ToString()), ("$category", key.CategoryId.ToString()), ("$level", key.LevelId.ToString())];

        internal static RaceRanking Read(PerformanceSession session, RaceKey key)
        {
            var args = Arguments(key);
            using var race = session.Command("SELECT IsClosed\nFROM RaceConfiguration\nWHERE DisciplineId = $discipline AND CategoryId = $category AND LevelId = $level;", args);
            var closedValue = race.ExecuteScalar() ?? throw new InvalidOperationException("La gara non esiste più.");
            List<Score> scores = [];
            using (var command = session.Command("SELECT ChoreographyId, JudgeId, CriterionId, Tenths\nFROM Score;"))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    scores.Add(new Score(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), Guid.Parse(reader.GetString(2)), reader.GetInt32(3)));
                }
            }

            List<PerformanceJudge> judges = [];
            using (var command = session.Command("SELECT ChoreographyId, JudgeId, IsRequired\nFROM PerformanceJudge;"))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    judges.Add(new PerformanceJudge(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), reader.GetBoolean(2)));
                }
            }

            List<Guid> criteria = [];
            using (var command = session.Command("SELECT Id\nFROM EvaluationCriterion;"))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    criteria.Add(Guid.Parse(reader.GetString(0)));
                }
            }

            var page = new ScorePage(judges, scores);
            List<RankingEntry> entries = [];
            using (var command = session.Command("""
                SELECT choreography.Id, choreography.Status, performance.Number, performance.Block, performance.OrderIndex, performance.IsConfirmed
                FROM Choreography AS choreography
                    LEFT JOIN Performance AS performance ON performance.ChoreographyId = choreography.Id
                WHERE choreography.DisciplineId = $discipline AND choreography.CategoryId = $category AND choreography.LevelId = $level
                ORDER BY performance.OrderIndex, choreography.Name COLLATE SKATING_NAME, choreography.Id;
                """, args))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Guid.Parse(reader.GetString(0));
                    entries.Add(new RankingEntry(id, reader.IsDBNull(4) ? entries.Count : reader.GetInt32(4), reader.IsDBNull(2) ? null : reader.GetInt32(2),
                        reader.IsDBNull(3) ? null : reader.GetInt32(3), !reader.IsDBNull(5) && reader.GetBoolean(5), (PerformanceStatus)reader.GetInt32(1),
                        ScoreCompletion.IsComplete(id, page, criteria), null, null, false, [.. scores.Where(score => score.ChoreographyId == id)], []));
                }
            }

            return RankingCalculator.Calculate(key, Convert.ToBoolean(closedValue), entries);
        }

        internal static void RequireOpenRace(PerformanceSession session, Guid id)
        {
            if (session.Scalar("""
                SELECT COUNT(*)
                FROM Choreography AS choreography
                    INNER JOIN RaceConfiguration AS race ON race.DisciplineId = choreography.DisciplineId
                        AND race.CategoryId = choreography.CategoryId AND race.LevelId = choreography.LevelId
                WHERE choreography.Id = $id AND race.IsClosed = 0;
                """, ("$id", id.ToString())) != 1)
            {
                throw new InvalidOperationException("Riaprire la gara dalla classifica prima di modificare l'esibizione.");
            }
        }
    }
}
