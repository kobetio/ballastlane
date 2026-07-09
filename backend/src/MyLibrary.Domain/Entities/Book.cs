using MyLibrary.Domain.Common;
using MyLibrary.Domain.Enums;

namespace MyLibrary.Domain.Entities;

/// <summary>
/// A book that belongs to exactly one user's personal library.
/// </summary>
public class Book
{
    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Author { get; private set; } = string.Empty;

    public string? Genre { get; private set; }

    public int? PublicationYear { get; private set; }

    public ReadingStatus? ReadingStatus { get; private set; }

    public int? Rating { get; private set; }

    public string? Notes { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>
    /// Reserved for EF Core materialization.
    /// </summary>
    private Book()
    {
    }

    public Book(
        string title,
        string author,
        Guid userId,
        string? genre = null,
        int? publicationYear = null,
        ReadingStatus? readingStatus = null,
        int? rating = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title));
        Author = Guard.AgainstNullOrWhiteSpace(author, nameof(author));
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        Genre = genre;
        PublicationYear = publicationYear;
        ReadingStatus = readingStatus;
        Rating = rating;
        Notes = notes;
    }

    /// <summary>
    /// Determines whether this book is owned by the given user. Used to enforce
    /// the "users can only access their own books" business rule.
    /// </summary>
    public bool BelongsTo(Guid userId) => UserId == userId;

    public void Update(
        string title,
        string author,
        string? genre,
        int? publicationYear,
        ReadingStatus? readingStatus,
        int? rating,
        string? notes)
    {
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title));
        Author = Guard.AgainstNullOrWhiteSpace(author, nameof(author));
        Genre = genre;
        PublicationYear = publicationYear;
        ReadingStatus = readingStatus;
        Rating = rating;
        Notes = notes;
    }
}
