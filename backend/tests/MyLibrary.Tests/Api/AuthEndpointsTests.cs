using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MyLibrary.Application.DTOs.Auth;

namespace MyLibrary.Tests.Api;

/// <summary>
/// End-to-end tests that hit the real HTTP pipeline (routing, the global validation filter,
/// and the global exception middleware) for the auth endpoints, verifying the full request
/// flow rather than just the isolated <c>AuthService</c>/<c>AuthController</c> units.
/// </summary>
public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Register_WithValidRequest_ReturnsCreatedWithToken()
    {
        var request = new RegisterRequest("Ada Lovelace", UniqueEmail(), "P@ssword123");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be(request.Email);
        body.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithInvalidRequest_ReturnsBadRequestWithFieldErrors()
    {
        var request = new RegisterRequest(string.Empty, "not-an-email", "123");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Name");
        problem.Errors.Should().ContainKey("Email");
        problem.Errors.Should().ContainKey("Password");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequestWithEmailError()
    {
        var email = UniqueEmail();
        var request = new RegisterRequest("Ada Lovelace", email, "P@ssword123");
        await _client.PostAsJsonAsync("/api/auth/register", request);

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Someone Else", email, "AnotherP@ss1"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var email = UniqueEmail();
        const string password = "P@ssword123";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Ada Lovelace", email, password));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be(email);
        body.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Ada Lovelace", email, "P@ssword123"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword1"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(UniqueEmail(), "P@ssword123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidRequest_ReturnsBadRequestWithFieldErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("not-an-email", string.Empty));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Email");
        problem.Errors.Should().ContainKey("Password");
    }
}
