using HackerRank1.Application;
using HackerRank1.Domain;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Infrastructure;

public class EfLibraryRepository : ILibraryRepository
{
    private readonly LibraryContext _context;

    public EfLibraryRepository(LibraryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Library>> GetAsync(int[]? ids, CancellationToken cancellationToken = default)
    {
        var query = _context.Libraries.AsQueryable();

        if (ids != null && ids.Length > 0)
            query = query.Where(x => ids.Contains(x.Id));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Library?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Libraries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Library> AddAsync(Library library, CancellationToken cancellationToken = default)
    {
        await _context.Libraries.AddAsync(library, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return library;
    }

    public async Task<Library> UpdateAsync(Library library, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Libraries.SingleAsync(x => x.Id == library.Id, cancellationToken);
        existing.Name = library.Name;
        existing.Location = library.Location;
        await _context.SaveChangesAsync(cancellationToken);
        return library;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var library = await _context.Libraries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (library == null)
            return false;

        _context.Libraries.Remove(library);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
