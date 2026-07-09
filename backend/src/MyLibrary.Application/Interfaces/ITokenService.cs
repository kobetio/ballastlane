using MyLibrary.Domain.Entities;

namespace MyLibrary.Application.Interfaces;

/// <summary>
/// A generated JWT and its expiration, returned by <see cref="ITokenService"/>.
/// </summary>
public record TokenResult(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    TokenResult GenerateToken(User user);
}
