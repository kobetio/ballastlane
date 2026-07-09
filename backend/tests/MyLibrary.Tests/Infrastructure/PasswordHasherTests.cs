using FluentAssertions;
using MyLibrary.Infrastructure.Security;

namespace MyLibrary.Tests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ProducesValueDifferentFromPlainTextPassword()
    {
        var hash = _sut.Hash("S3curePassword!");

        hash.Should().NotBe("S3curePassword!");
        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("S3curePassword!");

        _sut.Verify("S3curePassword!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("S3curePassword!");

        _sut.Verify("WrongPassword!", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_CalledTwiceWithSamePassword_ProducesDifferentHashes()
    {
        var hash1 = _sut.Hash("S3curePassword!");
        var hash2 = _sut.Hash("S3curePassword!");

        hash1.Should().NotBe(hash2);
    }
}
