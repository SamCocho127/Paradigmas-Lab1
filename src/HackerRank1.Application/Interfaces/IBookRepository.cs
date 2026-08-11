using HackerRank1.Domain.Entities;

namespace HackerRank1.Application.Interfaces;

public interface IBookRepository
{
    Task<IEnumerable<Book>> ListAsync(int libraryId, int[] ids);

    Task<Book> AddAsync(Book book);

    Task<Book> UpdateAsync(Book book);

    Task DeleteAsync(Book book);

    Task SaveChangesAsync();
}
