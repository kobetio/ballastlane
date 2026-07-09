using MyLibrary.Application.Common.Exceptions;
using MyLibrary.Application.DTOs.Auth;
using MyLibrary.Application.Interfaces;
using MyLibrary.Domain.Entities;

namespace MyLibrary.Application.Services;

/// <summary>
/// Implements the registration and login use cases.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new AppValidationException(nameof(RegisterRequest.Email), "Email is already registered.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Name, request.Email, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AuthenticationException("Invalid email or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        return BuildAuthResponse(user);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(user.Id, user.Name, user.Email, token.Token, token.ExpiresAtUtc);
    }
}
