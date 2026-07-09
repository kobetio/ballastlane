using FluentAssertions;
using MyLibrary.Domain.Entities;
using MyLibrary.Infrastructure.Persistence.Repositories;

namespace MyLibrary.Tests.Infrastructure;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteContextFactory _factory = new();
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        _sut = new UserRepository(_factory.Context);
    }

    [Fact]
    public async Task AddAsync_PersistsUser()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");

        await _sut.AddAsync(user);

        var stored = await _sut.GetByIdAsync(user.Id);
        stored.Should().NotBeNull();
        stored!.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        await _sut.AddAsync(user);

        var result = await _sut.GetByEmailAsync("jane@example.com");

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetByEmailAsync("missing@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailIsTaken_ReturnsTrue()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        await _sut.AddAsync(user);

        var exists = await _sut.EmailExistsAsync("jane@example.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailIsFree_ReturnsFalse()
    {
        var exists = await _sut.EmailExistsAsync("free@example.com");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    public void Dispose() => _factory.Dispose();
}
