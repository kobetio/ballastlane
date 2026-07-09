using MyLibrary.Domain.Enums;

namespace MyLibrary.Application.DTOs.Books;

/// <summary>
/// Payload used to add a new book to the current user's library.
/// </summary>
public record BookCreateRequest(
    string Title,
    string Author,
    string? Genre,
    int? PublicationYear,
    ReadingStatus? ReadingStatus,
    int? Rating,
    string? Notes) : IBookRequest;
