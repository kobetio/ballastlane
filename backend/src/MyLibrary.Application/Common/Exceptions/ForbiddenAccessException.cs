namespace MyLibrary.Application.Common.Exceptions;

/// <summary>
/// Thrown when a user attempts to access a resource they do not own. Maps to HTTP 403.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message) : base(message)
    {
    }
}
