namespace MyLibrary.Application.Common.Exceptions;

/// <summary>
/// Thrown when authentication credentials are missing or invalid. Maps to HTTP 401.
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message)
    {
    }
}
