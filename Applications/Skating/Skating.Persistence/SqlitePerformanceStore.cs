using Microsoft.Data.Sqlite;

using Skating.Core;

namespace Skating.Persistence
{
    /// <summary>Scaletta e voti locali. Le conferme conservano una copia tecnica prima della scrittura.</summary>
    public sealed class SqlitePerformanceStore(string databasePath, AwardAggregation? aggregation = null) : IPerformanceStore
    {
        private readonly SkatingDatabase _database = new(databasePath);

        /// <inheritdoc />
        public PerformanceSchedule GetSchedule()
        {
            using var session = new PerformanceSession(_database, true);
            var started = session.Scalar("SELECT IsStarted\nFROM EventState;") != 0;
            List<Performance> entries = [];
            using (var command = session.Command("""
                SELECT performance.ChoreographyId, performance.Number, performance.Block, performance.OrderIndex, performance.IsConfirmed, choreography.Status, race.IsClosed
                FROM Performance AS performance
                    INNER JOIN Choreography AS choreography ON choreography.Id = performance.ChoreographyId
                    INNER JOIN RaceConfiguration AS race ON race.DisciplineId = choreography.DisciplineId
                        AND race.CategoryId = choreography.CategoryId AND race.LevelId = choreography.LevelId
                ORDER BY performance.OrderIndex;
                """))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    entries.Add(new Performance(Guid.Parse(reader.GetString(0)), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetBoolean(4),
                        (PerformanceStatus)reader.GetInt32(5), reader.GetBoolean(6)));
                }
            }

            List<int> confirmed = [];
            using (var command = session.Command("SELECT Number\nFROM PerformanceBlock\nWHERE IsConfirmed = 1;"))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    confirmed.Add(reader.GetInt32(0));
                }
            }

            return new PerformanceSchedule(started, entries, confirmed);
        }

        /// <inheritdoc />
        public void Prepare(int blockSize)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(blockSize, 1);
            using var session = new PerformanceSession(_database);
            if (session.Scalar("SELECT IsStarted\nFROM EventState;") != 0 || session.Scalar("SELECT COUNT(*)\nFROM PerformanceBlock\nWHERE IsConfirmed = 1;") != 0)
            {
                throw new InvalidOperationException("I blocchi confermati non possono essere rigenerati. I nuovi iscritti ricevono blocchi supplementari.");
            }

            List<string> ids = [];
            using (var command = session.Command("""
                SELECT choreography.Id
                FROM Choreography AS choreography
                    LEFT JOIN Performance AS performance ON performance.ChoreographyId = choreography.Id
                ORDER BY COALESCE(performance.OrderIndex, 2147483647), choreography.Name COLLATE SKATING_NAME, choreography.Id;
                """))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetString(0));
                }
            }

            if (ids.Count == 0)
            {
                throw new InvalidOperationException("Inserire almeno una coreografia.");
            }

            session.Execute("DELETE\nFROM Performance; DELETE\nFROM PerformanceBlock;");
            for (var index = 0; index < ids.Count; index++)
            {
                var block = index / blockSize + 1;
                session.Execute("INSERT OR IGNORE INTO PerformanceBlock (Number) VALUES ($block);", ("$block", block));
                session.Execute("INSERT INTO Performance (ChoreographyId, Number, Block, OrderIndex) VALUES ($id, $number, $block, $number);",
                    ("$id", ids[index]), ("$number", index + 1), ("$block", block));
            }

            session.Execute("UPDATE EventState SET IsPrepared = 1;");
            session.Commit();
        }

        /// <inheritdoc />
        public void AssignBlock(Guid choreographyId, int block)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(block, 1);
            using var session = new PerformanceSession(_database);
            var original = session.Scalar("SELECT Block\nFROM Performance\nWHERE ChoreographyId = $id;", ("$id", choreographyId.ToString()));
            if (original == 0 || session.Scalar("SELECT COUNT(*)\nFROM PerformanceBlock\nWHERE Number IN ($source, $target) AND IsConfirmed = 1;", ("$source", original), ("$target", block)) != 0)
            {
                throw new InvalidOperationException("Scegliere un'esibizione e blocchi non ancora confermati.");
            }

            session.Execute("INSERT OR IGNORE INTO PerformanceBlock (Number) VALUES ($block);", ("$block", block));
            session.Execute("UPDATE Performance SET Block = $block\nWHERE ChoreographyId = $id;", ("$block", block), ("$id", choreographyId.ToString()));
            session.Execute("DELETE\nFROM PerformanceBlock\nWHERE Number NOT IN (SELECT Block\nFROM Performance);");
            session.Commit();
        }

        /// <inheritdoc />
        public void Move(Guid choreographyId, int direction)
        {
            if (direction is not (-1 or 1))
            {
                throw new ArgumentOutOfRangeException(nameof(direction));
            }

            using var session = new PerformanceSession(_database);
            var order = session.Scalar("SELECT OrderIndex\nFROM Performance\nWHERE ChoreographyId = $id;", ("$id", choreographyId.ToString()));
            if (order == 0)
            {
                throw new InvalidOperationException("Esibizione non presente.");
            }

            var target = order + direction;
            if (target >= 1 && target <= session.Scalar("SELECT COUNT(*)\nFROM Performance;"))
            {
                session.Execute("UPDATE Performance SET OrderIndex = $old\nWHERE OrderIndex = $target;", ("$old", order), ("$target", target));
                session.Execute("UPDATE Performance SET OrderIndex = $target\nWHERE ChoreographyId = $id;", ("$target", target), ("$id", choreographyId.ToString()));
            }

            session.Commit();
        }

        /// <inheritdoc />
        public void ConfirmBlocks()
        {
            using var session = new PerformanceSession(_database);
            if (session.Scalar("SELECT COUNT(*)\nFROM Performance;") == 0)
            {
                throw new InvalidOperationException("Preparare prima la scaletta.");
            }

            session.Execute("UPDATE PerformanceBlock SET IsConfirmed = 1;");
            session.Commit();
        }

        /// <inheritdoc />
        public void Start()
        {
            var races = new SqliteRaceStore(databasePath).GetRaces();
            var levels = new SqliteRegistryStore(databasePath).GetLevels().ToDictionary(entry => entry.Id);
            if (races.Count == 0 || races.Any(race => race.JudgeIds.Count < 3))
            {
                throw new InvalidOperationException("Ogni gara richiede almeno tre giudici.");
            }

            if (races.Where(race => levels[race.Key.LevelId].IsPro).Select(race => race.Key.CategoryId).Distinct().Count() > 1)
            {
                throw new InvalidOperationException("Il percorso PRO richiede una categoria unica.");
            }

            using var session = new PerformanceSession(_database);
            if (session.Scalar("SELECT IsStarted\nFROM EventState;") != 0)
            {
                return;
            }

            if (session.Scalar("SELECT COUNT(*)\nFROM Performance;") == 0 || session.Scalar("SELECT COUNT(*)\nFROM PerformanceBlock\nWHERE IsConfirmed = 0;") != 0
                || session.Scalar("SELECT COUNT(*)\nFROM EvaluationCriterion;") == 0)
            {
                throw new InvalidOperationException("Preparare e confermare tutti i blocchi e definire almeno un criterio prima dell'avvio.");
            }

            if (aggregation is AwardAggregation mode)
            {
                if (!Enum.IsDefined(mode))
                {
                    throw new ArgumentOutOfRangeException(nameof(aggregation));
                }

                var saved = session.Scalar("SELECT COALESCE(Aggregation, 0)\nFROM AwardSettings;");
                if (saved != 0 && saved != (int)mode)
                {
                    throw new InvalidOperationException("Il criterio premi è cambiato. Aprire Premi intermedi per confermare la configurazione prima dell'avvio.");
                }

                session.Execute("UPDATE AwardSettings SET Aggregation = $mode;", ("$mode", (int)mode));
            }

            session.Execute("UPDATE EventState SET IsStarted = 1;");
            session.Commit();
        }

        /// <inheritdoc />
        public ScorePage OpenPage(int block)
        {
            using var session = new PerformanceSession(_database);
            if (session.Scalar("SELECT IsStarted\nFROM EventState;") == 0 || session.Scalar("SELECT COUNT(*)\nFROM PerformanceBlock\nWHERE Number = $block AND IsConfirmed = 1;", ("$block", block)) == 0)
            {
                throw new InvalidOperationException("Avviare la manifestazione e confermare il blocco prima di inserire voti.");
            }

            if (session.Scalar("SELECT IsInitialized\nFROM PerformanceBlock\nWHERE Number = $block;", ("$block", block)) == 0)
            {
                session.Execute("""
                    INSERT INTO PerformanceJudge (ChoreographyId, JudgeId, IsRequired)
                    SELECT performance.ChoreographyId, judge.Id, 1 - judge.IsAbsent
                    FROM Performance AS performance
                        INNER JOIN Choreography AS choreography ON choreography.Id = performance.ChoreographyId
                        INNER JOIN RaceConfiguration AS race ON race.DisciplineId = choreography.DisciplineId
                            AND race.CategoryId = choreography.CategoryId AND race.LevelId = choreography.LevelId
                        CROSS JOIN Judge AS judge
                    WHERE performance.Block = $block AND (race.HasCustomJury = 0 OR EXISTS (
                        SELECT 1 FROM RaceJudge AS assignment
                        WHERE assignment.DisciplineId = race.DisciplineId AND assignment.CategoryId = race.CategoryId
                            AND assignment.LevelId = race.LevelId AND assignment.JudgeId = judge.Id));
                    UPDATE PerformanceBlock SET IsInitialized = 1 WHERE Number = $block;
                    """, ("$block", block));
            }

            var result = ReadPage(session, block);
            session.Commit();
            return result;
        }

        /// <inheritdoc />
        public void SaveScore(Guid choreographyId, Guid judgeId, Guid criterionId, int? tenths, bool allowAbsent = false)
        {
            if (tenths is < 0 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(tenths));
            }

            using var session = new PerformanceSession(_database);
            RequireOpen(session, choreographyId);
            var args = new (string, object)[] { ("$id", choreographyId.ToString()), ("$judge", judgeId.ToString()), ("$criterion", criterionId.ToString()) };
            if (session.Scalar("SELECT COUNT(*)\nFROM PerformanceJudge\nWHERE ChoreographyId = $id AND JudgeId = $judge;", args) == 0
                || session.Scalar("SELECT COUNT(*)\nFROM EvaluationCriterion\nWHERE Id = $criterion;", args) == 0)
            {
                throw new InvalidOperationException("Giudice o criterio non previsto dalla scheda.");
            }

            if (!allowAbsent && session.Scalar("SELECT IsRequired\nFROM PerformanceJudge\nWHERE ChoreographyId = $id AND JudgeId = $judge;", args) == 0)
            {
                throw new InvalidOperationException("Abilitare esplicitamente il giudice assente.");
            }

            if (tenths is null)
            {
                session.Execute("DELETE\nFROM Score\nWHERE ChoreographyId = $id AND JudgeId = $judge AND CriterionId = $criterion;", args);
            }
            else
            {
                session.Execute("UPDATE PerformanceJudge SET IsRequired = 1\nWHERE ChoreographyId = $id AND JudgeId = $judge;", args);
                session.Execute("""
                    INSERT INTO Score (ChoreographyId, JudgeId, CriterionId, Tenths) VALUES ($id, $judge, $criterion, $value)
                    ON CONFLICT (ChoreographyId, JudgeId, CriterionId) DO UPDATE SET Tenths = excluded.Tenths;
                    """, [.. args, ("$value", tenths.Value)]);
            }

            session.Commit();
        }

        /// <inheritdoc />
        public void SetJudgeRequired(Guid choreographyId, Guid judgeId, bool required)
        {
            using var session = new PerformanceSession(_database);
            RequireOpen(session, choreographyId);
            var args = new (string, object)[] { ("$id", choreographyId.ToString()), ("$judge", judgeId.ToString()), ("$required", required) };
            if (!required && session.Scalar("SELECT COUNT(*)\nFROM Score\nWHERE ChoreographyId = $id AND JudgeId = $judge;", args) != 0)
            {
                throw new InvalidOperationException("Il giudice ha già voti: svuotarli esplicitamente prima di segnalarlo assente.");
            }

            session.Execute("UPDATE PerformanceJudge SET IsRequired = $required\nWHERE ChoreographyId = $id AND JudgeId = $judge;", args);
            session.Commit();
        }

        /// <inheritdoc />
        public int ConfirmComplete(int block, Guid? choreographyId = null)
        {
            var page = OpenPage(block);
            var criteria = new SqliteRegistryStore(databasePath).GetCriteria().Select(entry => entry.Id).ToArray();
            var candidates = GetSchedule().Performances.Where(entry => entry.Block == block && !entry.IsConfirmed && !entry.IsRaceClosed && entry.Status == PerformanceStatus.Active
                && (choreographyId is null || entry.ChoreographyId == choreographyId)
                && ScoreCompletion.IsComplete(entry.ChoreographyId, page, criteria)).ToList();
            if (candidates.Count == 0)
            {
                return 0;
            }

            // Copia tecnica del prototipo: il catalogo di backup con ripristino sarà un blocco separato.
            using (var source = _database.Open(SqliteOpenMode.ReadOnly))
            using (var backup = new SkatingDatabase($"{databasePath}.before-confirm-{Guid.NewGuid():N}.bak").Open(SqliteOpenMode.ReadWriteCreate))
            {
                source.BackupDatabase(backup);
            }

            using var session = new PerformanceSession(_database);
            var current = ReadPage(session, block);
            var confirmed = 0;
            foreach (var candidate in candidates.Where(entry => ScoreCompletion.IsComplete(entry.ChoreographyId, current, criteria)))
            {
                RequireOpen(session, candidate.ChoreographyId);
                session.Execute("UPDATE Performance SET IsConfirmed = 1\nWHERE ChoreographyId = $id;", ("$id", candidate.ChoreographyId.ToString()));
                confirmed++;
            }

            session.Commit();
            return confirmed;
        }

        /// <inheritdoc />
        public void Reopen(Guid choreographyId)
        {
            using var session = new PerformanceSession(_database);
            RankingReader.RequireOpenRace(session, choreographyId);
            session.Execute("UPDATE Performance SET IsConfirmed = 0\nWHERE ChoreographyId = $id;", ("$id", choreographyId.ToString()));
            session.Commit();
        }

        private static void RequireOpen(PerformanceSession session, Guid id)
        {
            RankingReader.RequireOpenRace(session, id);
            if (session.Scalar("SELECT IsStarted\nFROM EventState;") == 0
                || session.Scalar("SELECT Status\nFROM Choreography\nWHERE Id = $id;", ("$id", id.ToString())) != 0
                || session.Scalar("SELECT COUNT(*)\nFROM Performance\nWHERE ChoreographyId = $id AND IsConfirmed = 0;", ("$id", id.ToString())) == 0)
            {
                throw new InvalidOperationException("La scheda non è aperta: riaprirla prima di modificare i voti.");
            }
        }

        private static ScorePage ReadPage(PerformanceSession session, int block)
        {
            List<PerformanceJudge> judges = [];
            using (var command = session.Command("""
                SELECT attendance.ChoreographyId, attendance.JudgeId, attendance.IsRequired
                FROM PerformanceJudge AS attendance
                    INNER JOIN Performance AS performance ON performance.ChoreographyId = attendance.ChoreographyId
                WHERE performance.Block = $block;
                """, ("$block", block)))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    judges.Add(new PerformanceJudge(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), reader.GetBoolean(2)));
                }
            }

            List<Score> scores = [];
            using (var command = session.Command("""
                SELECT score.ChoreographyId, score.JudgeId, score.CriterionId, score.Tenths
                FROM Score AS score
                    INNER JOIN Performance AS performance ON performance.ChoreographyId = score.ChoreographyId
                WHERE performance.Block = $block;
                """, ("$block", block)))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    scores.Add(new Score(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), Guid.Parse(reader.GetString(2)), reader.GetInt32(3)));
                }
            }

            return new ScorePage(judges, scores);
        }
    }
}
