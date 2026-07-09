using FluentAssertions;
using MyLibrary.Domain.Entities;
using MyLibrary.Domain.Exceptions;

namespace MyLibrary.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesUser()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");

        user.Id.Should().NotBe(Guid.Empty);
        user.Name.Should().Be("Jane Doe");
        user.Email.Should().Be("jane@example.com");
        user.PasswordHash.Should().Be("hashed-password");
        user.Books.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsDomainException(string? invalidName)
    {
        var act = () => new User(invalidName!, "jane@example.com", "hashed-password");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidEmail_ThrowsDomainException(string? invalidEmail)
    {
        var act = () => new User("Jane Doe", invalidEmail!, "hashed-password");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPasswordHash_ThrowsDomainException(string? invalidHash)
    {
        var act = () => new User("Jane Doe", "jane@example.com", invalidHash!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_GeneratesUniqueIdsForDifferentUsers()
    {
        var user1 = new User("Jane Doe", "jane@example.com", "hash1");
        var user2 = new User("John Doe", "john@example.com", "hash2");

        user1.Id.Should().NotBe(user2.Id);
    }
}
