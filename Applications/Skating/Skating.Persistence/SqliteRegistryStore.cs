using Microsoft.Data.Sqlite;

using Skating.Core;

namespace Skating.Persistence
{
    /// <summary>Anagrafiche locali: inserimenti parametrizzati e cataloghi univoci per nome, senza distinzione di maiuscole.</summary>
    public sealed class SqliteRegistryStore(string databasePath) : IRegistryStore
    {
        private readonly SkatingDatabase _database = new(databasePath);

        /// <inheritdoc />
        public IReadOnlyList<Judge> GetJudges() => Read("SELECT Id, Name, IsAbsent\nFROM Judge\nORDER BY Name COLLATE SKATING_NAME;", reader => new Judge(Guid.Parse(reader.GetString(0)), reader.GetString(1), reader.GetBoolean(2)));

        /// <inheritdoc />
        public void AddJudge(Judge judge)
        {
            ArgumentNullException.ThrowIfNull(judge);
            Write("INSERT INTO Judge (Id, Name, IsAbsent) VALUES ($id, $name, $absent);", judge.Id, judge.Name, command => command.Parameters.AddWithValue("$absent", judge.IsAbsent), requiresPreparation: true);
        }

        /// <inheritdoc />
        public void UpdateJudge(Judge judge)
        {
            ArgumentNullException.ThrowIfNull(judge);
            Write("UPDATE Judge SET Name = $name, IsAbsent = $absent\nWHERE Id = $id;", judge.Id, judge.Name, command => command.Parameters.AddWithValue("$absent", judge.IsAbsent));
        }

        /// <inheritdoc />
        public IReadOnlyList<EvaluationCriterion> GetCriteria() => Read("SELECT Id, Name\nFROM EvaluationCriterion\nORDER BY Name COLLATE SKATING_NAME;", reader => new EvaluationCriterion(Guid.Parse(reader.GetString(0)), reader.GetString(1)));

        /// <inheritdoc />
        public void AddCriterion(EvaluationCriterion criterion)
        {
            ArgumentNullException.ThrowIfNull(criterion);
            Write("INSERT INTO EvaluationCriterion (Id, Name) VALUES ($id, $name);", criterion.Id, criterion.Name, requiresPreparation: true);
        }

        /// <inheritdoc />
        public void UpdateCriterion(EvaluationCriterion criterion)
        {
            ArgumentNullException.ThrowIfNull(criterion);
            Write("UPDATE EvaluationCriterion SET Name = $name\nWHERE Id = $id;", criterion.Id, criterion.Name, requiresPreparation: true);
        }

        /// <inheritdoc />
        public void DeleteCriterion(Guid id) => Write("DELETE FROM EvaluationCriterion\nWHERE Id = $id;", id, string.Empty, requiresPreparation: true);

        /// <inheritdoc />
        public IReadOnlyList<School> GetSchools() => Read("SELECT Id, Name\nFROM School\nORDER BY Name COLLATE SKATING_NAME;", reader => new School(Guid.Parse(reader.GetString(0)), reader.GetString(1)));

        /// <inheritdoc />
        public void AddSchool(School school)
        {
            ArgumentNullException.ThrowIfNull(school);
            Write("INSERT INTO School (Id, Name) VALUES ($id, $name);", school.Id, school.Name);
        }

        /// <inheritdoc />
        public void UpdateSchool(School school)
        {
            ArgumentNullException.ThrowIfNull(school);
            Write("UPDATE School SET Name = $name\nWHERE Id = $id;", school.Id, school.Name);
        }

        /// <inheritdoc />
        public IReadOnlyList<Athlete> GetAthletes() => Read("SELECT Id, FirstName, LastName\nFROM Athlete\nORDER BY LastName COLLATE SKATING_NAME, FirstName COLLATE SKATING_NAME, Id;", reader => new Athlete(Guid.Parse(reader.GetString(0)), reader.GetString(1), reader.GetString(2)));

        /// <inheritdoc />
        public void AddAthlete(Athlete athlete)
        {
            ArgumentNullException.ThrowIfNull(athlete);
            Write("INSERT INTO Athlete (Id, FirstName, LastName) VALUES ($id, $name, $lastName);", athlete.Id, athlete.FirstName, command => command.Parameters.AddWithValue("$lastName", athlete.LastName));
        }

        /// <inheritdoc />
        public void UpdateAthlete(Athlete athlete)
        {
            ArgumentNullException.ThrowIfNull(athlete);
            Write("UPDATE Athlete SET FirstName = $name, LastName = $lastName\nWHERE Id = $id;", athlete.Id, athlete.FirstName, command => command.Parameters.AddWithValue("$lastName", athlete.LastName));
        }

        /// <inheritdoc />
        public IReadOnlyList<Discipline> GetDisciplines() => Read("SELECT Id, Name\nFROM Discipline\nORDER BY Name COLLATE SKATING_NAME;", reader => new Discipline(Guid.Parse(reader.GetString(0)), reader.GetString(1)));

        /// <inheritdoc />
        public void AddDiscipline(Discipline discipline)
        {
            ArgumentNullException.ThrowIfNull(discipline);
            Write("INSERT INTO Discipline (Id, Name) VALUES ($id, $name);", discipline.Id, discipline.Name);
        }

        /// <inheritdoc />
        public void UpdateDiscipline(Discipline discipline)
        {
            ArgumentNullException.ThrowIfNull(discipline);
            Write("UPDATE Discipline SET Name = $name\nWHERE Id = $id;", discipline.Id, discipline.Name);
        }

        /// <inheritdoc />
        public IReadOnlyList<Category> GetCategories() => Read("SELECT Id, Name\nFROM Category\nORDER BY Name COLLATE SKATING_NAME;", reader => new Category(Guid.Parse(reader.GetString(0)), reader.GetString(1)));

        /// <inheritdoc />
        public void AddCategory(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);
            Write("INSERT INTO Category (Id, Name) VALUES ($id, $name);", category.Id, category.Name);
        }

        /// <inheritdoc />
        public void UpdateCategory(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);
            Write("UPDATE Category SET Name = $name\nWHERE Id = $id;", category.Id, category.Name);
        }

        /// <inheritdoc />
        public IReadOnlyList<Level> GetLevels() => Read("SELECT Id, Name, IsPro\nFROM Level\nORDER BY Name COLLATE SKATING_NAME;", reader => new Level(Guid.Parse(reader.GetString(0)), reader.GetString(1), reader.GetBoolean(2)));

        /// <inheritdoc />
        public void AddLevel(Level level)
        {
            ArgumentNullException.ThrowIfNull(level);
            Write("INSERT INTO Level (Id, Name, IsPro) VALUES ($id, $name, $isPro);", level.Id, level.Name, command => command.Parameters.AddWithValue("$isPro", level.IsPro));
        }

        /// <inheritdoc />
        public void UpdateLevel(Level level)
        {
            ArgumentNullException.ThrowIfNull(level);
            Write("UPDATE Level SET Name = $name, IsPro = $isPro\nWHERE Id = $id;", level.Id, level.Name, command => command.Parameters.AddWithValue("$isPro", level.IsPro), changesAwardMembership: true);
        }

        private IReadOnlyList<T> Read<T>(string sql, Func<SqliteDataReader, T> map)
        {
            using var connection = _database.Open(SqliteOpenMode.ReadOnly);
            RequireRegistry(connection);
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            using var reader = command.ExecuteReader();
            List<T> result = [];
            while (reader.Read())
            {
                result.Add(map(reader));
            }

            return result;
        }

        private void Write(string sql, Guid id, string name, Action<SqliteCommand>? addParameters = null, bool requiresPreparation = false, bool changesAwardMembership = false)
        {
            using var connection = _database.Open(SqliteOpenMode.ReadWrite);
            RequireRegistry(connection);
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT IsStarted\nFROM EventState;";
            if (requiresPreparation && Convert.ToInt32(command.ExecuteScalar()) != 0)
            {
                throw new InvalidOperationException("Dopo l'avvio non è possibile modificare i criteri o aggiungere giudici.");
            }

            command.CommandText = sql;
            command.Parameters.AddWithValue("$id", id.ToString());
            command.Parameters.AddWithValue("$name", name);
            addParameters?.Invoke(command);
            if (changesAwardMembership)
            {
                command.CommandText = "SELECT COUNT(*)\nFROM Level\nWHERE Id = $id AND IsPro <> $isPro;";
                if (Convert.ToInt32(command.ExecuteScalar()) != 0)
                {
                    command.CommandText = "SELECT COUNT(*)\nFROM RaceConfiguration\nWHERE LevelId = $id AND IsClosed = 1;";
                    if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                    {
                        throw new InvalidOperationException("Riaprire le gare del livello prima di cambiare il percorso PRO.");
                    }

                    command.CommandText = """
                        UPDATE AbsoluteAward SET IsValidated = 0
                        WHERE EXISTS (SELECT 1 FROM RaceConfiguration WHERE LevelId = $id);
                        UPDATE IntermediateAward SET IsValidated = 0
                        WHERE (Aggregation = 1 AND GroupId IN (SELECT DisciplineId FROM RaceConfiguration WHERE LevelId = $id))
                            OR (Aggregation = 2 AND GroupId IN (SELECT CategoryId FROM RaceConfiguration WHERE LevelId = $id));
                        """;
                    command.ExecuteNonQuery();
                }

                command.CommandText = sql;
            }
            try
            {
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("La voce non esiste più. Annullare la modifica e aggiornare l'elenco.");
                }
                transaction.Commit();
            }
            catch (SqliteException exception) when (exception.SqliteExtendedErrorCode is 1555 or 2067)
            {
                throw new InvalidOperationException("Esiste già una voce con questo nome o identificativo nell'anagrafica selezionata.", exception);
            }
        }

        private static void RequireRegistry(SqliteConnection connection)
        {
            if (SkatingDatabase.ReadVersion(connection) < SkatingDatabase.SchemaVersion)
            {
                throw new InvalidOperationException("Aggiornare l'archivio prima di usare le anagrafiche.");
            }
        }
    }
}
