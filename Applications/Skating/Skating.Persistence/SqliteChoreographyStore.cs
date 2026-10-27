using Microsoft.Data.Sqlite;

using Skating.Core;

namespace Skating.Persistence
{
    /// <summary>Iscrizioni SQLite con vincoli referenziali e aggiornamenti atomici dei componenti.</summary>
    public sealed class SqliteChoreographyStore(string databasePath) : IChoreographyStore
    {
        private readonly SkatingDatabase _database = new(databasePath);

        /// <inheritdoc />
        public IReadOnlyList<Choreography> GetChoreographies()
        {
            using var connection = _database.Open(SqliteOpenMode.ReadOnly);
            using var transaction = connection.BeginTransaction(deferred: true);
            RequireSchema(connection, transaction);
            return Read(connection, transaction);
        }

        /// <inheritdoc />
        public void AddChoreography(Choreography choreography) => Save(choreography, false);

        /// <inheritdoc />
        public void UpdateChoreography(Choreography choreography) => Save(choreography, true);

        private void Save(Choreography choreography, bool update)
        {
            ArgumentNullException.ThrowIfNull(choreography);
            using var connection = _database.Open(SqliteOpenMode.ReadWrite);
            using var transaction = connection.BeginTransaction();
            RequireSchema(connection, transaction);
            var existing = Read(connection, transaction);
            var original = existing.SingleOrDefault(entry => entry.Id == choreography.Id);
            using (var guard = connection.CreateCommand())
            {
                guard.Transaction = transaction;
                guard.CommandText = """
                    SELECT COUNT(*)
                    FROM RaceConfiguration
                    WHERE IsClosed = 1 AND ((DisciplineId = $discipline AND CategoryId = $category AND LevelId = $level)
                        OR (DisciplineId = $oldDiscipline AND CategoryId = $oldCategory AND LevelId = $oldLevel));
                    """;
                guard.Parameters.AddWithValue("$discipline", choreography.DisciplineId.ToString());
                guard.Parameters.AddWithValue("$category", choreography.CategoryId.ToString());
                guard.Parameters.AddWithValue("$level", choreography.LevelId.ToString());
                guard.Parameters.AddWithValue("$oldDiscipline", original?.DisciplineId.ToString() ?? "");
                guard.Parameters.AddWithValue("$oldCategory", original?.CategoryId.ToString() ?? "");
                guard.Parameters.AddWithValue("$oldLevel", original?.LevelId.ToString() ?? "");
                if (Convert.ToInt32(guard.ExecuteScalar()) > 0)
                {
                    throw new InvalidOperationException("Riaprire la gara prima di modificarne le iscrizioni.");
                }

                guard.Parameters.Clear();
                guard.CommandText = "SELECT COUNT(*) FROM PerformanceJudge WHERE ChoreographyId = $id;";
                guard.Parameters.AddWithValue("$id", choreography.Id.ToString());
                if (original is not null && Convert.ToInt32(guard.ExecuteScalar()) > 0 && (original.SchoolId != choreography.SchoolId || original.DisciplineId != choreography.DisciplineId
                    || original.CategoryId != choreography.CategoryId || original.LevelId != choreography.LevelId || original.Composition != choreography.Composition
                    || !original.AthleteIds.ToHashSet().SetEquals(choreography.AthleteIds)))
                {
                    throw new InvalidOperationException("La scheda voti è già predisposta: in questo prototipo sono modificabili solo nome e durata.");
                }
            }
            if (update && !existing.Any(entry => entry.Id == choreography.Id))
            {
                throw new InvalidOperationException("La coreografia non esiste più. Aggiornare l'elenco.");
            }

            if (existing.Any(entry => entry.Id != choreography.Id && entry.DisciplineId == choreography.DisciplineId && entry.CategoryId == choreography.CategoryId
                && entry.LevelId == choreography.LevelId && string.Equals(entry.Name, choreography.Name, StringComparison.OrdinalIgnoreCase) && entry.AthleteIds.ToHashSet().SetEquals(choreography.AthleteIds)))
            {
                throw new InvalidOperationException("Esiste già una coreografia con questo nome e questi atleti nella stessa gara.");
            }

            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = update
                    ? """
                      UPDATE Choreography SET Name = $name, SchoolId = $school, DisciplineId = $discipline,
                          CategoryId = $category, LevelId = $level, Composition = $composition, DurationSeconds = $duration
                      WHERE Id = $id;
                      DELETE FROM ChoreographyAthlete
                      WHERE ChoreographyId = $id;
                      """
                    : """
                      INSERT INTO Choreography (Id, Name, SchoolId, DisciplineId, CategoryId, LevelId, Composition, DurationSeconds)
                      VALUES ($id, $name, $school, $discipline, $category, $level, $composition, $duration);
                      """;
                command.Parameters.AddWithValue("$id", choreography.Id.ToString());
                command.Parameters.AddWithValue("$name", choreography.Name);
                command.Parameters.AddWithValue("$school", choreography.SchoolId.ToString());
                command.Parameters.AddWithValue("$discipline", choreography.DisciplineId.ToString());
                command.Parameters.AddWithValue("$category", choreography.CategoryId.ToString());
                command.Parameters.AddWithValue("$level", choreography.LevelId.ToString());
                command.Parameters.AddWithValue("$composition", (int)choreography.Composition);
                command.Parameters.AddWithValue("$duration", choreography.DurationSeconds);
                command.ExecuteNonQuery();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO ChoreographyAthlete (ChoreographyId, AthleteId) VALUES ($id, $athlete);";
                command.Parameters.AddWithValue("$id", choreography.Id.ToString());
                var athleteParameter = command.Parameters.Add("$athlete", SqliteType.Text);
                foreach (var athleteId in choreography.AthleteIds)
                {
                    athleteParameter.Value = athleteId.ToString();
                    command.ExecuteNonQuery();
                }

                command.Parameters.Clear();
                command.CommandText = AwardInvalidation.Sql;
                command.Parameters.AddWithValue("$discipline", choreography.DisciplineId.ToString());
                command.Parameters.AddWithValue("$category", choreography.CategoryId.ToString());
                command.Parameters.AddWithValue("$level", choreography.LevelId.ToString());
                command.ExecuteNonQuery();
                if (original is not null)
                {
                    command.Parameters["$discipline"].Value = original.DisciplineId.ToString();
                    command.Parameters["$category"].Value = original.CategoryId.ToString();
                    command.Parameters["$level"].Value = original.LevelId.ToString();
                    command.ExecuteNonQuery();
                }

                command.Parameters.Clear();
                command.CommandText = SkatingDatabase.SynchronizeRaces;
                command.ExecuteNonQuery();
                command.Parameters.AddWithValue("$id", choreography.Id.ToString());
                command.CommandText = """
                    INSERT INTO PerformanceBlock (Number)
                    SELECT COALESCE(MAX(Number), 0) + 1
                    FROM PerformanceBlock
                    HAVING (SELECT IsPrepared FROM EventState) = 1
                        AND NOT EXISTS (SELECT 1 FROM Performance WHERE ChoreographyId = $id);
                    INSERT INTO Performance (ChoreographyId, Number, Block, OrderIndex)
                    SELECT $id, COALESCE(MAX(Number), 0) + 1, (SELECT MAX(Number) FROM PerformanceBlock), COALESCE(MAX(OrderIndex), 0) + 1
                    FROM Performance
                    HAVING (SELECT IsPrepared FROM EventState) = 1
                        AND NOT EXISTS (SELECT 1 FROM Performance WHERE ChoreographyId = $id);
                    """;
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
            {
                throw new InvalidOperationException("Salvataggio rifiutato: verificare che scuola, cataloghi e atleti esistano e che l'identificativo non sia già utilizzato.", exception);
            }
        }

        private static IReadOnlyList<Choreography> Read(SqliteConnection connection, SqliteTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT ChoreographyId, AthleteId\nFROM ChoreographyAthlete\nORDER BY AthleteId;";
            Dictionary<Guid, List<Guid>> members = [];
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Guid.Parse(reader.GetString(0));
                    if (!members.TryGetValue(id, out var athletes))
                    {
                        athletes = [];
                        members.Add(id, athletes);
                    }

                    athletes.Add(Guid.Parse(reader.GetString(1)));
                }
            }

            command.CommandText = """
                SELECT Id, Name, SchoolId, DisciplineId, CategoryId, LevelId, Composition, DurationSeconds
                FROM Choreography
                ORDER BY Name COLLATE SKATING_NAME, Id;
                """;
            using var entries = command.ExecuteReader();
            List<Choreography> result = [];
            while (entries.Read())
            {
                var id = Guid.Parse(entries.GetString(0));
                result.Add(new Choreography(id, entries.GetString(1), Guid.Parse(entries.GetString(2)), Guid.Parse(entries.GetString(3)),
                    Guid.Parse(entries.GetString(4)), Guid.Parse(entries.GetString(5)), (CompositionKind)entries.GetInt32(6), entries.GetInt32(7), members.GetValueOrDefault(id) ?? []));
            }

            return result;
        }

        private static void RequireSchema(SqliteConnection connection, SqliteTransaction transaction)
        {
            if (SkatingDatabase.ReadVersion(connection, transaction) < SkatingDatabase.SchemaVersion)
            {
                throw new InvalidOperationException("Aggiornare l'archivio prima di usare le coreografie.");
            }
        }
    }
}
