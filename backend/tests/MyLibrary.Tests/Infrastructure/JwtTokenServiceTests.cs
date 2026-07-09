using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MyLibrary.Domain.Entities;
using MyLibrary.Infrastructure.Security;

namespace MyLibrary.Tests.Infrastructure;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "this-is-a-test-only-signing-secret-with-enough-length",
            Issuer = "MyLibrary.Tests",
            Audience = "MyLibrary.Tests.Client",
            ExpiryMinutes = 30
        });
        _sut = new JwtTokenService(options);
    }

    [Fact]
    public void GenerateToken_ProducesTokenContainingUserClaims()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");

        var result = _sut.GenerateToken(user);

        result.Token.Should().NotBeNullOrWhiteSpace();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwt.Issuer.Should().Be("MyLibrary.Tests");
    }

    [Fact]
    public void GenerateToken_SetsExpiryBasedOnConfiguredMinutes()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        var before = DateTime.UtcNow;

        var result = _sut.GenerateToken(user);

        result.ExpiresAtUtc.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromSeconds(5));
    }
}
