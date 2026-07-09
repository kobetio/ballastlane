using MyLibrary.Domain.Exceptions;

namespace MyLibrary.Domain.Common;

/// <summary>
/// Small set of guard clauses used to protect core domain invariants.
/// </summary>
internal static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        return value;
    }

    public static Guid AgainstEmpty(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"{fieldName} is required.");
        }

        return value;
    }
}
