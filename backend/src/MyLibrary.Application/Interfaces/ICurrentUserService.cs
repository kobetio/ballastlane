namespace MyLibrary.Application.Interfaces;

/// <summary>
/// Exposes the identity of the currently authenticated user, resolved from the JWT
/// by an implementation living in the API layer (composition root).
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
}
