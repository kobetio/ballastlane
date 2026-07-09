using Microsoft.EntityFrameworkCore;
using MyLibrary.Application.Interfaces;
using MyLibrary.Domain.Entities;

namespace MyLibrary.Infrastructure.Persistence.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Book>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Books
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.Title)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _context.Books.AddAsync(book, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Book book, CancellationToken cancellationToken = default)
    {
        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
