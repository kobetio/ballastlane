using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MyLibrary.Api.Services;

namespace MyLibrary.Tests.Api.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
    private readonly CurrentUserService _sut;

    public CurrentUserServiceTests()
    {
        _sut = new CurrentUserService(_httpContextAccessor.Object);
    }

    [Fact]
    public void UserId_WhenNameIdentifierClaimPresent_ReturnsParsedGuid()
    {
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())]);
        _httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });

        _sut.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_WhenNoHttpContext_ReturnsNull()
    {
        _httpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        _sut.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenClaimMissing_ReturnsNull()
    {
        _httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) });

        _sut.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenClaimIsNotAGuid_ReturnsNull()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "not-a-guid")]);
        _httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });

        _sut.UserId.Should().BeNull();
    }
}
