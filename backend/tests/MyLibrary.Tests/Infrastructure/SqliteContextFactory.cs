using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyLibrary.Infrastructure.Persistence;

namespace MyLibrary.Tests.Infrastructure;

/// <summary>
/// Creates an <see cref="AppDbContext"/> backed by an in-memory SQLite database for
/// fast repository integration tests. The same EF Core model/configuration used
/// against PostgreSQL is exercised here; only the underlying provider differs.
/// The connection must stay open for the lifetime of the test (SQLite ":memory:"
/// databases are destroyed when the connection closes).
/// </summary>
public sealed class SqliteContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    public SqliteContextFactory()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
