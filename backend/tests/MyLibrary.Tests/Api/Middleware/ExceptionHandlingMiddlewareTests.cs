using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MyLibrary.Api.Middleware;
using MyLibrary.Application.Common.Exceptions;

namespace MyLibrary.Tests.Api.Middleware;

/// <summary>
/// Unit tests for <see cref="ExceptionHandlingMiddleware"/>, exercising it directly (without
/// a full HTTP pipeline) so every exception-to-status-code branch, including the unhandled/500
/// case that's hard to trigger from a real endpoint, is covered explicitly. Serialization of
/// each response body is also verified here, guarding against a regression like the one found
/// during manual testing where <c>ValidationProblemDetails.Errors</c> was silently dropped.
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(int StatusCode, JsonElement Body)> InvokeAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(_ => throw exception, NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        return (context.Response.StatusCode, JsonDocument.Parse(json).RootElement.Clone());
    }

    [Fact]
    public async Task NotFoundException_MapsTo404()
    {
        var (statusCode, body) = await InvokeAsync(new NotFoundException("Book not found."));

        statusCode.Should().Be(StatusCodes.Status404NotFound);
        body.GetProperty("status").GetInt32().Should().Be(404);
        body.GetProperty("detail").GetString().Should().Be("Book not found.");
    }

    [Fact]
    public async Task ForbiddenAccessException_MapsTo403()
    {
        var (statusCode, body) = await InvokeAsync(new ForbiddenAccessException("Not your book."));

        statusCode.Should().Be(StatusCodes.Status403Forbidden);
        body.GetProperty("status").GetInt32().Should().Be(403);
    }

    [Fact]
    public async Task AuthenticationException_MapsTo401()
    {
        var (statusCode, body) = await InvokeAsync(new AuthenticationException("Invalid email or password."));

        statusCode.Should().Be(StatusCodes.Status401Unauthorized);
        body.GetProperty("status").GetInt32().Should().Be(401);
    }

    [Fact]
    public async Task AppValidationException_MapsTo400WithFieldErrors()
    {
        var (statusCode, body) = await InvokeAsync(new AppValidationException("Email", "Email is already registered."));

        statusCode.Should().Be(StatusCodes.Status400BadRequest);
        body.GetProperty("errors").GetProperty("Email")[0].GetString().Should().Be("Email is already registered.");
    }

    [Fact]
    public async Task FluentValidationException_MapsTo400WithGroupedFieldErrors()
    {
        var failures = new[]
        {
            new FluentValidation.Results.ValidationFailure("Title", "Title is required."),
            new FluentValidation.Results.ValidationFailure("Title", "Title must not exceed 150 characters."),
            new FluentValidation.Results.ValidationFailure("Author", "Author is required.")
        };

        var (statusCode, body) = await InvokeAsync(new FluentValidation.ValidationException(failures));

        statusCode.Should().Be(StatusCodes.Status400BadRequest);
        var errors = body.GetProperty("errors");
        errors.GetProperty("Title").GetArrayLength().Should().Be(2);
        errors.GetProperty("Author").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task UnhandledException_MapsTo500WithGenericMessage()
    {
        var (statusCode, body) = await InvokeAsync(new InvalidOperationException("Some internal secret detail."));

        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        body.GetProperty("detail").GetString().Should().NotContain("Some internal secret detail.");
    }
}
