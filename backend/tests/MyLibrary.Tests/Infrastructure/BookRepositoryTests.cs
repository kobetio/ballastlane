using FluentAssertions;
using MyLibrary.Domain.Entities;
using MyLibrary.Domain.Enums;
using MyLibrary.Infrastructure.Persistence.Repositories;

namespace MyLibrary.Tests.Infrastructure;

public class BookRepositoryTests : IDisposable
{
    private readonly SqliteContextFactory _factory = new();
    private readonly BookRepository _sut;

    public BookRepositoryTests()
    {
        _sut = new BookRepository(_factory.Context);
    }

    /// <summary>
    /// Books have a required FK to Users, so tests must seed an owning user first.
    /// </summary>
    private async Task<Guid> SeedUserAsync()
    {
        var user = new User($"User {Guid.NewGuid()}", $"{Guid.NewGuid()}@example.com", "hashed-password");
        _factory.Context.Users.Add(user);
        await _factory.Context.SaveChangesAsync();
        return user.Id;
    }

    [Fact]
    public async Task AddAsync_PersistsBook()
    {
        var userId = await SeedUserAsync();
        var book = new Book("Clean Code", "Robert C. Martin", userId, "Software", 2008, ReadingStatus.Read, 5, "Great");

        await _sut.AddAsync(book);

        var stored = await _sut.GetByIdAsync(book.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Clean Code");
        stored.ReadingStatus.Should().Be(ReadingStatus.Read);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyBooksOwnedByThatUser()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync();
        await _sut.AddAsync(new Book("Clean Code", "Robert C. Martin", userId));
        await _sut.AddAsync(new Book("The Pragmatic Programmer", "Andrew Hunt", userId));
        await _sut.AddAsync(new Book("Refactoring", "Martin Fowler", otherUserId));

        var result = await _sut.GetByUserIdAsync(userId);

        result.Should().HaveCount(2);
        result.Select(b => b.UserId).Should().OnlyContain(id => id == userId);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var userId = await SeedUserAsync();
        var book = new Book("Clean Code", "Robert C. Martin", userId);
        await _sut.AddAsync(book);

        book.Update("Clean Code (2nd ed.)", "Robert C. Martin", "Software", 2008, ReadingStatus.Reading, 4, "Updated");
        await _sut.UpdateAsync(book);

        var stored = await _sut.GetByIdAsync(book.Id);
        stored!.Title.Should().Be("Clean Code (2nd ed.)");
        stored.Rating.Should().Be(4);
    }

    [Fact]
    public async Task DeleteAsync_RemovesBook()
    {
        var userId = await SeedUserAsync();
        var book = new Book("Clean Code", "Robert C. Martin", userId);
        await _sut.AddAsync(book);

        await _sut.DeleteAsync(book);

        var stored = await _sut.GetByIdAsync(book.Id);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    public void Dispose() => _factory.Dispose();
}
