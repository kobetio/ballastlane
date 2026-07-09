using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyLibrary.Application.Interfaces;
using MyLibrary.Application.Services;
using MyLibrary.Application.Validators;

namespace MyLibrary.Application;

/// <summary>
/// Composition helper that registers Application services and every FluentValidation
/// validator defined in this assembly. Called once from the API's composition root.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookService, BookService>();

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}
