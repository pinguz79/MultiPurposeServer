using Microsoft.Data.Sqlite;

namespace Skating.Persistence
{
    internal sealed class SkatingDatabase(string databasePath)
    {
        internal const int ApplicationId = 1397445716;
        internal const int SchemaVersion = 8;

        internal const string AbsoluteAwardSchema = """
            CREATE TABLE AbsoluteAward (
                Kind INTEGER PRIMARY KEY CHECK (Kind IN (0, 1)),
                IsValidated INTEGER NOT NULL DEFAULT 0 CHECK (IsValidated IN (0, 1))
            );
            INSERT INTO AbsoluteAward (Kind) VALUES (0), (1);
            """;

        internal const string AwardSchema = """
            CREATE TABLE AwardSettings (Singleton INTEGER PRIMARY KEY CHECK (Singleton = 1), Aggregation INTEGER NULL CHECK (Aggregation IN (1, 2)));
            INSERT INTO AwardSettings (Singleton) VALUES (1);
            CREATE TABLE IntermediateAward (
                Aggregation INTEGER NOT NULL CHECK (Aggregation IN (1, 2)),
                GroupId TEXT NOT NULL,
                IsValidated INTEGER NOT NULL DEFAULT 0 CHECK (IsValidated IN (0, 1)),
                PRIMARY KEY (Aggregation, GroupId)
            );
            """;

        internal const string RankingSchema = """
            ALTER TABLE RaceConfiguration ADD COLUMN IsClosed INTEGER NOT NULL DEFAULT 0 CHECK (IsClosed IN (0, 1));
            ALTER TABLE Choreography ADD COLUMN Status INTEGER NOT NULL DEFAULT 0 CHECK (Status IN (0, 1, 2));
            """;

        internal const string PerformanceSchema = """
            ALTER TABLE Judge ADD COLUMN IsAbsent INTEGER NOT NULL DEFAULT 0 CHECK (IsAbsent IN (0, 1));
            CREATE TABLE EventState (Singleton INTEGER PRIMARY KEY CHECK (Singleton = 1), IsStarted INTEGER NOT NULL DEFAULT 0, IsPrepared INTEGER NOT NULL DEFAULT 0);
            INSERT INTO EventState (Singleton) VALUES (1);
            CREATE TABLE PerformanceBlock (Number INTEGER PRIMARY KEY CHECK (Number > 0), IsConfirmed INTEGER NOT NULL DEFAULT 0, IsInitialized INTEGER NOT NULL DEFAULT 0);
            CREATE TABLE Performance (
                ChoreographyId TEXT PRIMARY KEY REFERENCES Choreography(Id),
                Number INTEGER NOT NULL UNIQUE,
                Block INTEGER NOT NULL REFERENCES PerformanceBlock(Number),
                OrderIndex INTEGER NOT NULL,
                IsConfirmed INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE PerformanceJudge (
                ChoreographyId TEXT NOT NULL REFERENCES Performance(ChoreographyId),
                JudgeId TEXT NOT NULL REFERENCES Judge(Id),
                IsRequired INTEGER NOT NULL,
                PRIMARY KEY (ChoreographyId, JudgeId)
            );
            CREATE TABLE Score (
                ChoreographyId TEXT NOT NULL,
                JudgeId TEXT NOT NULL,
                CriterionId TEXT NOT NULL REFERENCES EvaluationCriterion(Id),
                Tenths INTEGER NOT NULL CHECK (Tenths BETWEEN 0 AND 100),
                PRIMARY KEY (ChoreographyId, JudgeId, CriterionId),
                FOREIGN KEY (ChoreographyId, JudgeId) REFERENCES PerformanceJudge(ChoreographyId, JudgeId)
            );
            """;

        internal const string RaceSchema = """
            CREATE TABLE Judge (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL COLLATE SKATING_NAME UNIQUE CHECK (length(trim(Name)) > 0)
            );
            CREATE TABLE EvaluationCriterion (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL COLLATE SKATING_NAME UNIQUE CHECK (length(trim(Name)) > 0)
            );
            INSERT INTO EvaluationCriterion (Id, Name) VALUES
                ('59f61392-1055-4c56-87c3-96ee3a050b11', 'Tecnica'),
                ('59f61392-1055-4c56-87c3-96ee3a050b12', 'Interpretazione / Musicalità'),
                ('59f61392-1055-4c56-87c3-96ee3a050b13', 'Show');
            CREATE TABLE RaceConfiguration (
                DisciplineId TEXT NOT NULL REFERENCES Discipline(Id),
                CategoryId TEXT NOT NULL REFERENCES Category(Id),
                LevelId TEXT NOT NULL REFERENCES Level(Id),
                HasCustomJury INTEGER NOT NULL DEFAULT 0 CHECK (HasCustomJury IN (0, 1)),
                PRIMARY KEY (DisciplineId, CategoryId, LevelId)
            );
            CREATE TABLE RaceJudge (
                DisciplineId TEXT NOT NULL,
                CategoryId TEXT NOT NULL,
                LevelId TEXT NOT NULL,
                JudgeId TEXT NOT NULL REFERENCES Judge(Id),
                PRIMARY KEY (DisciplineId, CategoryId, LevelId, JudgeId),
                FOREIGN KEY (DisciplineId, CategoryId, LevelId)
                    REFERENCES RaceConfiguration(DisciplineId, CategoryId, LevelId) ON DELETE CASCADE
            );
            """;

        internal const string SynchronizeRaces = """
            INSERT OR IGNORE INTO RaceConfiguration (DisciplineId, CategoryId, LevelId)
            SELECT DISTINCT DisciplineId, CategoryId, LevelId
            FROM Choreography;
            DELETE FROM RaceConfiguration
            WHERE NOT EXISTS (
                SELECT 1
                FROM Choreography
                WHERE Choreography.DisciplineId = RaceConfiguration.DisciplineId
                    AND Choreography.CategoryId = RaceConfiguration.CategoryId
                    AND Choreography.LevelId = RaceConfiguration.LevelId
            );
            """;

        internal const string ChoreographySchema = """
            CREATE TABLE School (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL COLLATE SKATING_NAME UNIQUE CHECK (length(trim(Name)) > 0)
            );
            CREATE TABLE Choreography (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL COLLATE SKATING_NAME CHECK (length(trim(Name)) > 0),
                SchoolId TEXT NOT NULL REFERENCES School(Id),
                DisciplineId TEXT NOT NULL REFERENCES Discipline(Id),
                CategoryId TEXT NOT NULL REFERENCES Category(Id),
                LevelId TEXT NOT NULL REFERENCES Level(Id),
                Composition INTEGER NOT NULL CHECK (Composition IN (1, 2, 3)),
                DurationSeconds INTEGER NOT NULL CHECK (DurationSeconds >= 0)
            );
            CREATE TABLE ChoreographyAthlete (
                ChoreographyId TEXT NOT NULL REFERENCES Choreography(Id),
                AthleteId TEXT NOT NULL REFERENCES Athlete(Id),
                PRIMARY KEY (ChoreographyId, AthleteId)
            );
            CREATE INDEX IX_ChoreographyAthlete_Athlete ON ChoreographyAthlete (AthleteId);
            """;

        internal const string RegistrySchema = """
            CREATE TABLE Athlete (
                Id TEXT PRIMARY KEY NOT NULL,
                FirstName TEXT NOT NULL CHECK (length(trim(FirstName)) > 0),
                LastName TEXT NOT NULL CHECK (length(trim(LastName)) > 0)
            );
            CREATE TABLE Discipline (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL COLLATE SKATING_NAME UNIQUE CHECK (length(trim(Name)) > 0)
            );
            CREATE TABLE Category (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL COLLATE SKATING_NAME UNIQUE CHECK (length(trim(Name)) > 0)
            );
            CREATE TABLE Level (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL COLLATE SKATING_NAME UNIQUE CHECK (length(trim(Name)) > 0),
                IsPro INTEGER NOT NULL CHECK (IsPro IN (0, 1))
            );
            """;

        internal SqliteConnection Open(SqliteOpenMode mode)
        {
            var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = databasePath, Mode = mode, Pooling = false, DefaultTimeout = 2, ForeignKeys = true,
            }.ToString());

            try
            {
                connection.Open();
                connection.CreateCollation("SKATING_NAME", (left, right) => string.Compare(left, right, StringComparison.OrdinalIgnoreCase));
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        internal static int ReadVersion(SqliteConnection connection, SqliteTransaction? transaction = null)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "PRAGMA application_id;";
            if (Convert.ToInt32(command.ExecuteScalar()) != ApplicationId)
            {
                throw new InvalidDataException("Il file non è un archivio del nuovo Skating System.");
            }

            command.CommandText = "PRAGMA user_version;";
            var version = Convert.ToInt32(command.ExecuteScalar());
            if (version is < 1 or > SchemaVersion)
            {
                throw new InvalidDataException("La versione dell'archivio non è supportata da questa applicazione.");
            }

            return version;
        }
    }
}
