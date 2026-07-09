using MyLibrary.Domain.Common;

namespace MyLibrary.Domain.Entities;

/// <summary>
/// A registered user who owns a personal collection of books.
/// </summary>
public class User
{
    private readonly List<Book> _books = [];

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// The books owned by this user.
    /// </summary>
    public IReadOnlyCollection<Book> Books => _books.AsReadOnly();

    /// <summary>
    /// Reserved for EF Core materialization.
    /// </summary>
    private User()
    {
    }

    public User(string name, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email));
        PasswordHash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
    }
}
