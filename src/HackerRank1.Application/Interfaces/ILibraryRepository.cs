using HackerRank1.Domain.Entities;

namespace HackerRank1.Application.Interfaces;

public interface ILibraryRepository
{
    Task<IEnumerable<Library>> ListAsync(int[] ids);

    Task<Library?> GetByIdAsync(int id);

    Task<Library> AddAsync(Library library);

    Task<Library> UpdateAsync(Library library);

    Task DeleteAsync(Library library);

    Task SaveChangesAsync();
}
