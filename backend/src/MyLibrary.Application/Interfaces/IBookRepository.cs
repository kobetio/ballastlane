using MyLibrary.Domain.Entities;

namespace MyLibrary.Application.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Book>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Book book, CancellationToken cancellationToken = default);

    Task UpdateAsync(Book book, CancellationToken cancellationToken = default);

    Task DeleteAsync(Book book, CancellationToken cancellationToken = default);
}
