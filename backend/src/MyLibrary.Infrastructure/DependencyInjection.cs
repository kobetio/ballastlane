using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLibrary.Application.Interfaces;
using MyLibrary.Infrastructure.Persistence;
using MyLibrary.Infrastructure.Persistence.Repositories;
using MyLibrary.Infrastructure.Security;

namespace MyLibrary.Infrastructure;

/// <summary>
/// Composition helper that registers every Infrastructure-provided implementation
/// of an Application interface. Called once from the API's composition root.
/// </summary>
public static class DependencyInjection
{
    /// <param name="registerDbContext">
    /// Set to <see langword="false"/> in test hosts that need to register <see cref="AppDbContext"/>
    /// themselves against a different provider (e.g. in-memory SQLite); registering it here as well
    /// would make EF Core see two providers configured for the same context and throw.
    /// </param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool registerDbContext = true)
    {
        if (registerDbContext)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
