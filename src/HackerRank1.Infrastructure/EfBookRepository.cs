using HackerRank1.Application;
using HackerRank1.Domain;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Infrastructure;

public class EfBookRepository : IBookRepository
{
    private readonly LibraryContext _context;

    public EfBookRepository(LibraryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetByLibraryAsync(int libraryId, int[]? ids, CancellationToken cancellationToken = default)
    {
        var query = _context.Books.AsQueryable().Where(b => b.LibraryId == libraryId);

        if (ids != null && ids.Length > 0)
            query = query.Where(b => ids.Contains(b.Id));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _context.Books.AddAsync(book, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return book;
    }

    public async Task<Book> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Books.SingleAsync(b => b.Id == book.Id, cancellationToken);
        existing.Name = book.Name;
        existing.Category = book.Category;
        await _context.SaveChangesAsync(cancellationToken);
        return book;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (book == null)
            return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
