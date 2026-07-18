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
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PortfolioContext>().UseSqlite(_connection).EnableSensitiveDataLogging().Options;

        DbContext = new PortfolioContext(options);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}