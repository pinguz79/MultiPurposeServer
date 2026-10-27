using Microsoft.Data.Sqlite;

using Skating.Core;

namespace Skating.Persistence
{
    /// <summary>Configurazione locale delle giurie; le gare sono sincronizzate dal salvataggio delle iscrizioni.</summary>
    public sealed class SqliteRaceStore(string databasePath) : IRaceStore
    {
        private readonly SkatingDatabase _database = new(databasePath);

        /// <inheritdoc />
        public RaceRanking GetRanking(RaceKey key)
        {
            using var session = new PerformanceSession(_database, true);
            return RankingReader.Read(session, key);
        }

        /// <inheritdoc />
        public void Close(RaceKey key)
        {
            using var session = new PerformanceSession(_database);
            if (!RankingReader.Read(session, key).CanClose)
            {
                throw new InvalidOperationException("Confermare tutte le schede attive e risolvere le parità sul podio prima di chiudere la gara.");
            }

            session.Execute("UPDATE RaceConfiguration SET IsClosed = 1\nWHERE DisciplineId = $discipline AND CategoryId = $category AND LevelId = $level;", RankingReader.Arguments(key));
            session.Commit();
        }

        /// <inheritdoc />
        public void Reopen(RaceKey key)
        {
            using var session = new PerformanceSession(_database);
            RankingReader.Read(session, key);
            AwardInvalidation.ForRace(session, key);
            session.Execute("UPDATE RaceConfiguration SET IsClosed = 0\nWHERE DisciplineId = $discipline AND CategoryId = $category AND LevelId = $level;", RankingReader.Arguments(key));
            session.Commit();
        }

        /// <inheritdoc />
        public void SetStatus(Guid choreographyId, PerformanceStatus status)
        {
            if (!Enum.IsDefined(status))
            {
                throw new ArgumentOutOfRangeException(nameof(status));
            }

            using var session = new PerformanceSession(_database);
            RankingReader.RequireOpenRace(session, choreographyId);
            var previous = session.Scalar("SELECT Status\nFROM Choreography\nWHERE Id = $id;", ("$id", choreographyId.ToString()));
            if (previous != (int)status)
            {
                session.Execute("UPDATE Choreography SET Status = $status\nWHERE Id = $id;", ("$status", (int)status), ("$id", choreographyId.ToString()));
                session.Execute("UPDATE Performance SET IsConfirmed = 0\nWHERE ChoreographyId = $id;", ("$id", choreographyId.ToString()));
            }

            session.Commit();
        }

        /// <inheritdoc />
        public IReadOnlyList<Race> GetRaces()
        {
            using var connection = _database.Open(SqliteOpenMode.ReadOnly);
            using var transaction = connection.BeginTransaction(deferred: true);
            RequireSchema(connection, transaction);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT Id\nFROM Judge\nORDER BY Name COLLATE SKATING_NAME;";
            List<Guid> allJudges = [];
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    allJudges.Add(Guid.Parse(reader.GetString(0)));
                }
            }

            command.CommandText = "SELECT DisciplineId, CategoryId, LevelId, JudgeId\nFROM RaceJudge;";
            List<(RaceKey Key, Guid JudgeId)> assignments = [];
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    assignments.Add((ReadKey(reader), Guid.Parse(reader.GetString(3))));
                }
            }

            command.CommandText = """
                SELECT race.DisciplineId, race.CategoryId, race.LevelId, race.HasCustomJury, choreography.Id
                FROM RaceConfiguration AS race
                    INNER JOIN Choreography AS choreography ON choreography.DisciplineId = race.DisciplineId
                        AND choreography.CategoryId = race.CategoryId AND choreography.LevelId = race.LevelId;
                """;
            List<(RaceKey Key, bool IsCustom, Guid ChoreographyId)> rows = [];
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rows.Add((ReadKey(reader), reader.GetBoolean(3), Guid.Parse(reader.GetString(4))));
                }
            }

            return [.. rows.GroupBy(row => row.Key).Select(group => new Race(group.Key, Array.AsReadOnly(group.Select(row => row.ChoreographyId).ToArray()),
                Array.AsReadOnly(group.First().IsCustom ? assignments.Where(entry => entry.Key == group.Key).Select(entry => entry.JudgeId).ToArray() : allJudges.ToArray()), group.First().IsCustom))];
        }

        /// <inheritdoc />
        public void SetJury(RaceKey key, IReadOnlyCollection<Guid> judgeIds)
        {
            ArgumentNullException.ThrowIfNull(judgeIds);
            if (judgeIds.Count < 3 || judgeIds.Distinct().Count() != judgeIds.Count || judgeIds.Contains(Guid.Empty))
            {
                throw new ArgumentException("La giuria deve comprendere almeno tre giudici distinti.", nameof(judgeIds));
            }

            SaveJury(key, judgeIds);
        }

        /// <inheritdoc />
        public void UseDefaultJury(RaceKey key) => SaveJury(key, null);

        private void SaveJury(RaceKey key, IReadOnlyCollection<Guid>? judgeIds)
        {
            ArgumentNullException.ThrowIfNull(key);
            using var connection = _database.Open(SqliteOpenMode.ReadWrite);
            using var transaction = connection.BeginTransaction();
            RequireSchema(connection, transaction);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.Parameters.AddWithValue("$discipline", key.DisciplineId.ToString());
            command.Parameters.AddWithValue("$category", key.CategoryId.ToString());
            command.Parameters.AddWithValue("$level", key.LevelId.ToString());
            command.Parameters.AddWithValue("$custom", judgeIds is not null);
            command.CommandText = """
                SELECT COUNT(*)
                FROM PerformanceJudge AS attendance
                    INNER JOIN Choreography AS choreography ON choreography.Id = attendance.ChoreographyId
                WHERE choreography.DisciplineId = $discipline AND choreography.CategoryId = $category AND choreography.LevelId = $level;
                """;
            if (Convert.ToInt32(command.ExecuteScalar()) > 0)
            {
                throw new InvalidOperationException("La scheda voti di questa gara è già predisposta. Gestire le assenze senza cambiare la giuria.");
            }

            command.CommandText = """
                UPDATE RaceConfiguration SET HasCustomJury = $custom
                WHERE DisciplineId = $discipline AND CategoryId = $category AND LevelId = $level;
                """;
            if (command.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException("La gara non esiste più. Aggiornare l'elenco.");
            }

            command.CommandText = """
                DELETE FROM RaceJudge
                WHERE DisciplineId = $discipline AND CategoryId = $category AND LevelId = $level;
                """;
            command.ExecuteNonQuery();
            if (judgeIds is not null)
            {
                command.CommandText = "INSERT INTO RaceJudge (DisciplineId, CategoryId, LevelId, JudgeId) VALUES ($discipline, $category, $level, $judge);";
                var parameter = command.Parameters.Add("$judge", SqliteType.Text);
                try
                {
                    foreach (var id in judgeIds)
                    {
                        parameter.Value = id.ToString();
                        command.ExecuteNonQuery();
                    }
                }
                catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
                {
                    throw new InvalidOperationException("Uno dei giudici non è più disponibile. Aggiornare l'elenco.", exception);
                }
            }

            transaction.Commit();
        }

        private static RaceKey ReadKey(SqliteDataReader reader) => new(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), Guid.Parse(reader.GetString(2)));

        private static void RequireSchema(SqliteConnection connection, SqliteTransaction transaction)
        {
            if (SkatingDatabase.ReadVersion(connection, transaction) < SkatingDatabase.SchemaVersion)
            {
                throw new InvalidOperationException("Aggiornare l'archivio prima di configurare le gare.");
            }
        }
    }
}
