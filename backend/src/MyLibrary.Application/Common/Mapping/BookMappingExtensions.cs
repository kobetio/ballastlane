using MyLibrary.Application.DTOs.Books;
using MyLibrary.Domain.Entities;

namespace MyLibrary.Application.Common.Mapping;

public static class BookMappingExtensions
{
    public static BookResponse ToResponse(this Book book) => new(
        book.Id,
        book.Title,
        book.Author,
        book.Genre,
        book.PublicationYear,
        book.ReadingStatus,
        book.Rating,
        book.Notes);
}
