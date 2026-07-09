using MyLibrary.Domain.Enums;

namespace MyLibrary.Application.DTOs.Books;

/// <summary>
/// Representation of a book returned to API clients.
/// </summary>
public record BookResponse(
    Guid Id,
    string Title,
    string Author,
    string? Genre,
    int? PublicationYear,
    ReadingStatus? ReadingStatus,
    int? Rating,
    string? Notes);
