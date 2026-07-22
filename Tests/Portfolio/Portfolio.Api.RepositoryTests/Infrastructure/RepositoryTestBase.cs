using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;

namespace Portfolio.Api.RepositoryTests.Infrastructure;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;

    protected PortfolioContext DbContext { get; }

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        DbContext = CreateContext(_connection);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    protected static async Task<SqliteConnection> CreateInitializedConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        return connection;
    }

    protected static PortfolioContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<PortfolioContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        return new PortfolioContext(options);
    }

    protected static async Task<T?> GetPersistedValue<T>(SqliteConnection connection, Guid id, string table, string field)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {field} FROM {table} WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        var value = await command.ExecuteScalarAsync();

        if (value is null or DBNull)
        {
            return default;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }
}
