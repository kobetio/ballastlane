using Microsoft.AspNetCore.Identity;
using MyLibrary.Application.Interfaces;
using MyLibrary.Domain.Entities;

namespace MyLibrary.Infrastructure.Security;

/// <summary>
/// Wraps ASP.NET Core Identity's battle-tested PBKDF2-based hasher so we don't
/// hand-roll password hashing.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(user: null!, password);

    public bool Verify(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(user: null!, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
