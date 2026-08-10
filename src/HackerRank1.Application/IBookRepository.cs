using HackerRank1.Domain;

namespace HackerRank1.Application;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetByLibraryAsync(int libraryId, int[]? ids, CancellationToken cancellationToken = default);

    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default);

    Task<Book> UpdateAsync(Book book, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
