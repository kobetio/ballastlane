namespace MyLibrary.Application.DTOs.Auth;

/// <summary>
/// Payload used to authenticate an existing user.
/// </summary>
public record LoginRequest(string Email, string Password);
