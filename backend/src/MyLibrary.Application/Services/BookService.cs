using MyLibrary.Application.Common.Exceptions;
using MyLibrary.Application.Common.Mapping;
using MyLibrary.Application.DTOs.Books;
using MyLibrary.Application.Interfaces;
using MyLibrary.Domain.Entities;

namespace MyLibrary.Application.Services;

/// <summary>
/// Implements the Books use cases, including the "users can only access their own
/// books" ownership rule (Specification.md §4).
/// </summary>
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IReadOnlyList<BookResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var books = await _bookRepository.GetByUserIdAsync(userId, cancellationToken);
        return books.Select(b => b.ToResponse()).ToList();
    }

    public async Task<BookResponse> GetByIdAsync(Guid bookId, Guid userId, CancellationToken cancellationToken = default)
    {
        var book = await GetOwnedBookOrThrowAsync(bookId, userId, cancellationToken);
        return book.ToResponse();
    }

    public async Task<BookResponse> CreateAsync(Guid userId, BookCreateRequest request, CancellationToken cancellationToken = default)
    {
        var book = new Book(
            request.Title,
            request.Author,
            userId,
            request.Genre,
            request.PublicationYear,
            request.ReadingStatus,
            request.Rating,
            request.Notes);

        await _bookRepository.AddAsync(book, cancellationToken);

        return book.ToResponse();
    }

    public async Task<BookResponse> UpdateAsync(Guid bookId, Guid userId, BookUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var book = await GetOwnedBookOrThrowAsync(bookId, userId, cancellationToken);

        book.Update(
            request.Title,
            request.Author,
            request.Genre,
            request.PublicationYear,
            request.ReadingStatus,
            request.Rating,
            request.Notes);

        await _bookRepository.UpdateAsync(book, cancellationToken);

        return book.ToResponse();
    }

    public async Task DeleteAsync(Guid bookId, Guid userId, CancellationToken cancellationToken = default)
    {
        var book = await GetOwnedBookOrThrowAsync(bookId, userId, cancellationToken);
        await _bookRepository.DeleteAsync(book, cancellationToken);
    }

    private async Task<Book> GetOwnedBookOrThrowAsync(Guid bookId, Guid userId, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken)
            ?? throw new NotFoundException($"Book with id '{bookId}' was not found.");

        if (!book.BelongsTo(userId))
        {
            throw new ForbiddenAccessException("You do not have permission to access this book.");
        }

        return book;
    }
}
