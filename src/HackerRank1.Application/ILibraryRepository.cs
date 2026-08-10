using HackerRank1.Domain;

namespace HackerRank1.Application;

public interface ILibraryRepository
{
    Task<IEnumerable<Library>> GetAsync(int[]? ids, CancellationToken cancellationToken = default);

    Task<Library?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Library> AddAsync(Library library, CancellationToken cancellationToken = default);

    Task<Library> UpdateAsync(Library library, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
