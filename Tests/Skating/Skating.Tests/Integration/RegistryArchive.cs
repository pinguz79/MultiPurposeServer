using Microsoft.Data.Sqlite;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    internal sealed class RegistryArchive : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-registry-tests-").FullName;

        internal string DatabasePath => Path.Combine(_directory, "demo.skating");

        internal IReadOnlyList<string> Backups => Directory.GetFiles(_directory, "*.bak");

        internal void Create() => new SqliteManifestationStore(DatabasePath).Create(new Manifestation(Guid.NewGuid(), "Manifestazione prova"));

        internal void CreateLegacy()
        {
            Execute("""
                PRAGMA application_id = 1397445716;
                PRAGMA user_version = 1;
                CREATE TABLE Manifestation (Singleton INTEGER PRIMARY KEY CHECK (Singleton = 1), Id TEXT NOT NULL, Name TEXT NOT NULL);
                INSERT INTO Manifestation (Singleton, Id, Name) VALUES (1, '2aaec385-f6e1-4669-910f-0275e67e4059', 'Manifestazione precedente');
                """);
        }

        internal void Execute(string sql)
        {
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = DatabasePath, Pooling = false }.ToString());
            connection.Open();
            connection.CreateCollation("SKATING_NAME", (left, right) => string.Compare(left, right, StringComparison.OrdinalIgnoreCase));
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public void Dispose() => Directory.Delete(_directory, true);
    }
}
