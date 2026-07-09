namespace MyLibrary.Domain.Exceptions;

/// <summary>
/// Thrown when an operation would violate a core invariant of a domain entity.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
