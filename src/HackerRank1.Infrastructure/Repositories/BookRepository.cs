using HackerRank1.Application.Interfaces;
using HackerRank1.Domain.Entities;
using HackerRank1.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryContext _libraryContext;

    public BookRepository(LibraryContext libraryContext)
    {
        _libraryContext = libraryContext;
    }

    public async Task<IEnumerable<Book>> ListAsync(int libraryId, int[] ids)
    {
        var query = _libraryContext.Books.AsQueryable().Where(b => b.LibraryId == libraryId);

        if (ids != null && ids.Any())
            query = query.Where(b => ids.Contains(b.Id));

        return await query.ToListAsync();
    }

    public async Task<Book> AddAsync(Book book)
    {
        await _libraryContext.Books.AddAsync(book);
        return book;
    }

    public Task<Book> UpdateAsync(Book book)
    {
        _libraryContext.Books.Update(book);
        return Task.FromResult(book);
    }

    public Task DeleteAsync(Book book)
    {
        _libraryContext.Books.Remove(book);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _libraryContext.SaveChangesAsync();
}
