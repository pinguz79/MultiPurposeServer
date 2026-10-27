using Microsoft.Data.Sqlite;

using Skating.Core;

namespace Skating.Persistence
{
    /// <summary>Archivio SQLite del prototipo. Rifiuta archivi estranei e versioni non supportate senza modificarli.</summary>
    public sealed class SqliteManifestationStore(string databasePath) : IManifestationStore
    {
        private readonly SkatingDatabase _database = new(databasePath);

        /// <inheritdoc />
        public void Create(Manifestation manifestation)
        {
            ArgumentNullException.ThrowIfNull(manifestation);

            // CreateNew riserva il nome senza sovrascrivere un file scelto per errore.
            using (var file = new FileStream(databasePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
            }

            // Un errore lascia il file riservato per diagnosi: nessuna cancellazione implicita.
            using var connection = _database.Open(SqliteOpenMode.ReadWrite);
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"""
                PRAGMA application_id = {SkatingDatabase.ApplicationId};
                PRAGMA user_version = {SkatingDatabase.SchemaVersion};
                CREATE TABLE Manifestation (
                    Singleton INTEGER PRIMARY KEY CHECK (Singleton = 1),
                    Id TEXT NOT NULL,
                    Name TEXT NOT NULL CHECK (length(trim(Name)) > 0)
                );
                INSERT INTO Manifestation (Singleton, Id, Name) VALUES (1, $id, $name);
                {SkatingDatabase.RegistrySchema}
                {SkatingDatabase.ChoreographySchema}
                {SkatingDatabase.RaceSchema}
                {SkatingDatabase.PerformanceSchema}
                {SkatingDatabase.RankingSchema}
                {SkatingDatabase.AwardSchema}
                {SkatingDatabase.AbsoluteAwardSchema}
                """;
            command.Parameters.AddWithValue("$id", manifestation.Id.ToString());
            command.Parameters.AddWithValue("$name", manifestation.Name);
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        /// <inheritdoc />
        public Manifestation Load()
        {
            try
            {
                using var connection = _database.Open(SqliteOpenMode.ReadOnly);
                SkatingDatabase.ReadVersion(connection);
                using var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Id, Name
                    FROM Manifestation
                    WHERE Singleton = 1;
                    """;
                using var reader = command.ExecuteReader();
                if (!reader.Read() || reader.IsDBNull(0) || reader.IsDBNull(1) || !Guid.TryParse(reader.GetString(0), out var id) || id == Guid.Empty || string.IsNullOrWhiteSpace(reader.GetString(1)))
                {
                    throw new InvalidDataException("L'archivio non contiene una manifestazione valida.");
                }

                return new Manifestation(id, reader.GetString(1));
            }
            catch (SqliteException exception)
            {
                throw new InvalidDataException("Impossibile leggere l'archivio. Verificare che esista e sia un database Skating valido.", exception);
            }
        }

        /// <inheritdoc />
        public bool NeedsUpgrade()
        {
            using var connection = _database.Open(SqliteOpenMode.ReadOnly);
            return SkatingDatabase.ReadVersion(connection) < SkatingDatabase.SchemaVersion;
        }

        /// <inheritdoc />
        public void Upgrade(UpgradeBackupMode backupMode = UpgradeBackupMode.CreateBackup)
        {
            if (!Enum.IsDefined(backupMode))
            {
                throw new ArgumentOutOfRangeException(nameof(backupMode));
            }

            Load();
            using var connection = _database.Open(SqliteOpenMode.ReadWrite);
            if (SkatingDatabase.ReadVersion(connection) == SkatingDatabase.SchemaVersion)
            {
                return;
            }

            if (backupMode == UpgradeBackupMode.CreateBackup)
            {
                // Il backup SQLite è consistente anche quando il database ha un journal: non si copia alla cieca il file.
                var backupPath = $"{databasePath}.before-v{SkatingDatabase.SchemaVersion}-{Guid.NewGuid():N}.bak";
                using var backup = new SkatingDatabase(backupPath).Open(SqliteOpenMode.ReadWriteCreate);
                connection.BackupDatabase(backup);
            }

            using var transaction = connection.BeginTransaction();
            var version = SkatingDatabase.ReadVersion(connection, transaction);
            if (version == SkatingDatabase.SchemaVersion)
            {
                return;
            }

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"""
                {(version < 2 ? SkatingDatabase.RegistrySchema : string.Empty)}
                {(version < 3 ? SkatingDatabase.ChoreographySchema : string.Empty)}
                {(version < 4 ? SkatingDatabase.RaceSchema : string.Empty)}
                {(version < 5 ? SkatingDatabase.PerformanceSchema : string.Empty)}
                {(version < 6 ? SkatingDatabase.RankingSchema : string.Empty)}
                {(version < 7 ? SkatingDatabase.AwardSchema : string.Empty)}
                {SkatingDatabase.AbsoluteAwardSchema}
                {SkatingDatabase.SynchronizeRaces}
                PRAGMA user_version = {SkatingDatabase.SchemaVersion};
                """;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
    }
}
