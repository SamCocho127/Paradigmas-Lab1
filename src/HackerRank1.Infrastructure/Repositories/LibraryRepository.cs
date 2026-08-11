using HackerRank1.Application.Interfaces;
using HackerRank1.Domain.Entities;
using HackerRank1.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Infrastructure.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly LibraryContext _libraryContext;

    public LibraryRepository(LibraryContext libraryContext)
    {
        _libraryContext = libraryContext;
    }

    public async Task<IEnumerable<Library>> ListAsync(int[] ids)
    {
        var query = _libraryContext.Libraries.AsQueryable();

        if (ids != null && ids.Any())
            query = query.Where(x => ids.Contains(x.Id));

        return await query.ToListAsync();
    }

    public async Task<Library?> GetByIdAsync(int id)
    {
        return await _libraryContext.Libraries.SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Library> AddAsync(Library library)
    {
        await _libraryContext.Libraries.AddAsync(library);
        return library;
    }

    public Task<Library> UpdateAsync(Library library)
    {
        _libraryContext.Libraries.Update(library);
        return Task.FromResult(library);
    }

    public Task DeleteAsync(Library library)
    {
        _libraryContext.Libraries.Remove(library);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _libraryContext.SaveChangesAsync();
}
