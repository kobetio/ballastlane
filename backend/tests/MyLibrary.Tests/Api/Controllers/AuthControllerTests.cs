using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyLibrary.Api.Controllers;
using MyLibrary.Application.DTOs.Auth;
using MyLibrary.Application.Interfaces;

namespace MyLibrary.Tests.Api.Controllers;

/// <summary>
/// Verifies <see cref="AuthController"/> delegates to <see cref="IAuthService"/> and maps
/// its results to the expected HTTP status codes. Validation/exception-to-status-code
/// mapping is covered end-to-end in <see cref="AuthEndpointsTests"/>.
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_authService.Object);
    }

    [Fact]
    public async Task Register_DelegatesToAuthServiceAndReturns201()
    {
        var request = new RegisterRequest("Jane Doe", "jane@example.com", "S3curePassword!");
        var expected = new AuthResponse(Guid.NewGuid(), request.Name, request.Email, "jwt-token", DateTime.UtcNow.AddHours(1));
        _authService.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.Register(request, CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        objectResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Login_DelegatesToAuthServiceAndReturns200()
    {
        var request = new LoginRequest("jane@example.com", "S3curePassword!");
        var expected = new AuthResponse(Guid.NewGuid(), "Jane Doe", request.Email, "jwt-token", DateTime.UtcNow.AddHours(1));
        _authService.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.Login(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }
}
