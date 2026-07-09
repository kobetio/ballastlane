using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLibrary.Infrastructure.Persistence;

namespace MyLibrary.Tests.Api;

/// <summary>
/// Boots the real <c>MyLibrary.Api</c> pipeline (routing, auth, validation filter, exception
/// middleware) for end-to-end HTTP tests, swapping the PostgreSQL-backed <see cref="AppDbContext"/>
/// for an in-memory SQLite one and supplying self-contained JWT settings, so tests never touch the
/// developer's real database or secrets.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "Integration-Test-Only-Signing-Key-1234567890-ABCDEFGH",
                ["Jwt:Issuer"] = "MyLibrary.Api.Tests",
                ["Jwt:Audience"] = "MyLibrary.Client.Tests",
                ["Jwt:ExpiryMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Program.cs skips registering AppDbContext against PostgreSQL when the
            // environment is "Testing" (set above), so this is the only DbContext
            // registration active in the container.
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
