using Microsoft.Data.Sqlite;

namespace Skating.Persistence
{
    internal sealed class PerformanceSession : IDisposable
    {
        internal SqliteConnection Connection { get; }

        internal SqliteTransaction Transaction { get; }

        internal PerformanceSession(SkatingDatabase database, bool readOnly = false)
        {
            Connection = database.Open(readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite);
            try
            {
                Transaction = Connection.BeginTransaction(deferred: readOnly);
                if (SkatingDatabase.ReadVersion(Connection, Transaction) != SkatingDatabase.SchemaVersion)
                {
                    throw new InvalidOperationException("Aggiornare l'archivio.");
                }
            }
            catch
            {
                Connection.Dispose();
                throw;
            }
        }

        internal SqliteCommand Command(string sql, params (string Name, object Value)[] values)
        {
            var command = Connection.CreateCommand();
            command.Transaction = Transaction;
            command.CommandText = sql;
            foreach (var (name, value) in values)
            {
                command.Parameters.AddWithValue(name, value);
            }

            return command;
        }

        internal long Scalar(string sql, params (string Name, object Value)[] values)
        {
            using var command = Command(sql, values);
            return Convert.ToInt64(command.ExecuteScalar());
        }

        internal void Execute(string sql, params (string Name, object Value)[] values)
        {
            using var command = Command(sql, values);
            command.ExecuteNonQuery();
        }

        internal void Commit() => Transaction.Commit();

        public void Dispose()
        {
            Transaction.Dispose();
            Connection.Dispose();
        }
    }
}
