namespace MyLibrary.Application.DTOs.Auth;

/// <summary>
/// Payload used to register a new user.
/// </summary>
public record RegisterRequest(string Name, string Email, string Password);
