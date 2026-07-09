using MyLibrary.Domain.Enums;

namespace MyLibrary.Application.DTOs.Books;

/// <summary>
/// Payload used to update an existing book owned by the current user.
/// </summary>
public record BookUpdateRequest(
    string Title,
    string Author,
    string? Genre,
    int? PublicationYear,
    ReadingStatus? ReadingStatus,
    int? Rating,
    string? Notes) : IBookRequest;
