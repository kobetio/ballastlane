using MyLibrary.Domain.Enums;

namespace MyLibrary.Application.DTOs.Books;

/// <summary>
/// Common shape shared by create/update book payloads, so both can reuse the same
/// FluentValidation rules.
/// </summary>
public interface IBookRequest
{
    string Title { get; }

    string Author { get; }

    string? Genre { get; }

    int? PublicationYear { get; }

    ReadingStatus? ReadingStatus { get; }

    int? Rating { get; }

    string? Notes { get; }
}
