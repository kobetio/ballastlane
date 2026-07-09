namespace MyLibrary.Application.DTOs.Auth;

/// <summary>
/// Result of a successful register/login, containing the issued JWT and basic user info.
/// </summary>
public record AuthResponse(Guid UserId, string Name, string Email, string Token, DateTime ExpiresAtUtc);
