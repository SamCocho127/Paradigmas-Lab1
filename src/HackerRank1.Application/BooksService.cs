using HackerRank1.Domain;

namespace HackerRank1.Application;

public interface IBooksService
{
    Task<IEnumerable<Book>> Get(int libraryId, int[]? ids);

    Task<Book?> GetById(int bookId);

    Task<Book> Add(Book book);

    Task<Book> Update(Book book);

    Task<bool> Delete(int bookId);
}

public class BooksService : IBooksService
{
    private readonly IBookRepository _bookRepository;

    public BooksService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public Task<IEnumerable<Book>> Get(int libraryId, int[]? ids)
        => _bookRepository.GetByLibraryAsync(libraryId, ids);

    public Task<Book?> GetById(int bookId)
        => _bookRepository.GetByIdAsync(bookId);

    public Task<Book> Add(Book book)
        => _bookRepository.AddAsync(book);

    public Task<Book> Update(Book book)
        => _bookRepository.UpdateAsync(book);

    public Task<bool> Delete(int bookId)
        => _bookRepository.DeleteAsync(bookId);
}
