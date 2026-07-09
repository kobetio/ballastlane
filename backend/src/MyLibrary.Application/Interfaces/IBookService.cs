using MyLibrary.Application.DTOs.Books;

namespace MyLibrary.Application.Interfaces;

public interface IBookService
{
    Task<IReadOnlyList<BookResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<BookResponse> GetByIdAsync(Guid bookId, Guid userId, CancellationToken cancellationToken = default);

    Task<BookResponse> CreateAsync(Guid userId, BookCreateRequest request, CancellationToken cancellationToken = default);

    Task<BookResponse> UpdateAsync(Guid bookId, Guid userId, BookUpdateRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid bookId, Guid userId, CancellationToken cancellationToken = default);
}
