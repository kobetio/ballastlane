using FluentAssertions;
using Moq;
using MyLibrary.Application.Common.Exceptions;
using MyLibrary.Application.DTOs.Auth;
using MyLibrary.Application.Interfaces;
using MyLibrary.Application.Services;
using MyLibrary.Domain.Entities;

namespace MyLibrary.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepository.Object, _passwordHasher.Object, _tokenService.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserAndReturnsToken()
    {
        var request = new RegisterRequest("Jane Doe", "jane@example.com", "S3curePassword!");
        _userRepository.Setup(r => r.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(request.Password)).Returns("hashed-password");
        _tokenService.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns(new TokenResult("jwt-token", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.RegisterAsync(request);

        result.Name.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
        result.Token.Should().Be("jwt-token");
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "jane@example.com" && u.PasswordHash == "hashed-password"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsAppValidationException()
    {
        var request = new RegisterRequest("Jane Doe", "jane@example.com", "S3curePassword!");
        _userRepository.Setup(r => r.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<AppValidationException>();
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        var request = new LoginRequest("jane@example.com", "S3curePassword!");
        _userRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(true);
        _tokenService.Setup(t => t.GenerateToken(user))
            .Returns(new TokenResult("jwt-token", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.LoginAsync(request);

        result.UserId.Should().Be(user.Id);
        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsAuthenticationException()
    {
        var request = new LoginRequest("jane@example.com", "S3curePassword!");
        _userRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsAuthenticationException()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        var request = new LoginRequest("jane@example.com", "wrong-password");
        _userRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(false);

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<AuthenticationException>();
    }
}
