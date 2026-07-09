using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MyLibrary.Application.DTOs.Auth;
using MyLibrary.Application.DTOs.Books;
using MyLibrary.Domain.Enums;

namespace MyLibrary.Tests.Api;

/// <summary>
/// End-to-end tests for the Books CRUD endpoints, exercising the real HTTP pipeline
/// (JWT authentication, ownership authorization, validation, and exception middleware)
/// against an in-memory SQLite database.
/// </summary>
public class BooksEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    // Mirrors the JsonStringEnumConverter registered in Program.cs, so deserializing
    // responses here matches how the real API serializes enum values (as their name,
    // e.g. "Read", rather than the default numeric value).
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly CustomWebApplicationFactory _factory;

    public BooksEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string UniqueEmail() => $"{Guid.NewGuid():N}@example.com";

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test User", UniqueEmail(), "P@ssword123"));
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    private static BookCreateRequest SampleCreateRequest() =>
        new("Dune", "Frank Herbert", "Sci-Fi", 1965, ReadingStatus.Read, 5, "A classic.");

    [Fact]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/books");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithNoBooks_ReturnsEmptyList()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/books");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var books = await response.Content.ReadFromJsonAsync<List<BookResponse>>(JsonOptions);
        books.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedWithLocationAndBook()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/books", SampleCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var book = await response.Content.ReadFromJsonAsync<BookResponse>(JsonOptions);
        book.Should().NotBeNull();
        book!.Title.Should().Be("Dune");
        book.Author.Should().Be("Frank Herbert");
    }

    [Fact]
    public async Task Create_WithInvalidRequest_ReturnsBadRequestWithFieldErrors()
    {
        var client = await CreateAuthenticatedClientAsync();
        var invalidRequest = new BookCreateRequest(string.Empty, string.Empty, null, 999, null, 6, null);

        var response = await client.PostAsJsonAsync("/api/books", invalidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Title");
        problem.Errors.Should().ContainKey("Author");
        problem.Errors.Should().ContainKey("PublicationYear");
        problem.Errors.Should().ContainKey("Rating");
    }

    [Fact]
    public async Task GetById_ForOwnBook_ReturnsOk()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/books", SampleCreateRequest())).Content.ReadFromJsonAsync<BookResponse>(JsonOptions);

        var response = await client.GetAsync($"/api/books/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ForUnknownId_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/books/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ForAnotherUsersBook_ReturnsForbidden()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var created = await (await ownerClient.PostAsJsonAsync("/api/books", SampleCreateRequest())).Content.ReadFromJsonAsync<BookResponse>(JsonOptions);

        var otherUserClient = await CreateAuthenticatedClientAsync();
        var response = await otherUserClient.GetAsync($"/api/books/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_ForAnotherUsersBook_ReturnsForbidden()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var created = await (await ownerClient.PostAsJsonAsync("/api/books", SampleCreateRequest())).Content.ReadFromJsonAsync<BookResponse>(JsonOptions);

        var otherUserClient = await CreateAuthenticatedClientAsync();
        var updateRequest = new BookUpdateRequest("New Title", "New Author", null, null, null, null, null);
        var response = await otherUserClient.PutAsJsonAsync($"/api/books/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_ForOwnBook_ReturnsOkWithUpdatedFields()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/books", SampleCreateRequest())).Content.ReadFromJsonAsync<BookResponse>(JsonOptions);
        var updateRequest = new BookUpdateRequest("Dune Messiah", "Frank Herbert", "Sci-Fi", 1969, ReadingStatus.Read, 4, "Sequel.");

        var response = await client.PutAsJsonAsync($"/api/books/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<BookResponse>(JsonOptions);
        updated!.Title.Should().Be("Dune Messiah");
        updated.Rating.Should().Be(4);
    }

    [Fact]
    public async Task Delete_ForAnotherUsersBook_ReturnsForbidden()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var created = await (await ownerClient.PostAsJsonAsync("/api/books", SampleCreateRequest())).Content.ReadFromJsonAsync<BookResponse>(JsonOptions);

        var otherUserClient = await CreateAuthenticatedClientAsync();
        var response = await otherUserClient.DeleteAsync($"/api/books/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_ForOwnBook_ReturnsNoContentAndRemovesIt()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/books", SampleCreateRequest())).Content.ReadFromJsonAsync<BookResponse>(JsonOptions);

        var deleteResponse = await client.DeleteAsync($"/api/books/{created!.Id}");
        var getResponse = await client.GetAsync($"/api/books/{created.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_OnlyReturnsBooksOwnedByCurrentUser()
    {
        var userAClient = await CreateAuthenticatedClientAsync();
        await userAClient.PostAsJsonAsync("/api/books", SampleCreateRequest());
        await userAClient.PostAsJsonAsync("/api/books", SampleCreateRequest() with { Title = "Second Book" });

        var userBClient = await CreateAuthenticatedClientAsync();
        await userBClient.PostAsJsonAsync("/api/books", SampleCreateRequest() with { Title = "User B's Book" });

        var userABooks = await (await userAClient.GetAsync("/api/books")).Content.ReadFromJsonAsync<List<BookResponse>>(JsonOptions);
        var userBBooks = await (await userBClient.GetAsync("/api/books")).Content.ReadFromJsonAsync<List<BookResponse>>(JsonOptions);

        userABooks.Should().HaveCount(2);
        userBBooks.Should().HaveCount(1);
        userBBooks![0].Title.Should().Be("User B's Book");
    }
}
