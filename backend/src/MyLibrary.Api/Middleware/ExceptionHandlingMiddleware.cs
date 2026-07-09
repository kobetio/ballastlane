using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MyLibrary.Application.Common.Exceptions;
using AppAuthenticationException = MyLibrary.Application.Common.Exceptions.AuthenticationException;

namespace MyLibrary.Api.Middleware;

/// <summary>
/// Catches every exception that escapes the MVC pipeline and translates it into a
/// consistent <see cref="ProblemDetails"/> JSON response with the appropriate status
/// code, so controllers never need try/catch blocks for expected failure cases.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        ProblemDetails response;

        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ValidationProblemDetails(ToErrorDictionary(validationException))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Instance = context.Request.Path
                };
                break;

            case AppValidationException appValidationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ValidationProblemDetails(appValidationException.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Instance = context.Request.Path
                };
                break;

            case AppAuthenticationException authenticationException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = authenticationException.Message,
                    Instance = context.Request.Path
                };
                break;

            case ForbiddenAccessException forbiddenAccessException:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                response = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = forbiddenAccessException.Message,
                    Instance = context.Request.Path
                };
                break;

            case NotFoundException notFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = notFoundException.Message,
                    Instance = context.Request.Path
                };
                break;

            default:
                _logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = "Please try again later or contact support if the problem persists.",
                    Instance = context.Request.Path
                };
                break;
        }

        // Serialize using the runtime type (e.g. ValidationProblemDetails) rather than the
        // declared ProblemDetails type, otherwise System.Text.Json drops derived-only members
        // such as ValidationProblemDetails.Errors.
        await context.Response.WriteAsJsonAsync(response, response.GetType());
    }

    private static Dictionary<string, string[]> ToErrorDictionary(ValidationException exception) =>
        exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());
}
