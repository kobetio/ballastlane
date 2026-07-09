namespace MyLibrary.Application.Common.Exceptions;

/// <summary>
/// Thrown by Application services when a state-dependent business rule is violated
/// (e.g. "email already registered") that FluentValidation's stateless validators can't
/// express on their own. Carries per-field friendly messages so it can be mapped to the
/// same HTTP 400 response shape as regular validation errors.
/// </summary>
public class AppValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException(string fieldName, string message) : base(message)
    {
        Errors = new Dictionary<string, string[]> { [fieldName] = [message] };
    }
}
