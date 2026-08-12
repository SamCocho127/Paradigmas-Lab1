using LibraryService.SharedKernel.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Modules.Books.Features;

public static class ListBooks
{
    public static async Task<IEnumerable<Book>?> Query(LibraryContext context, int libraryId)
    {
        var libraryExists = await context.Libraries.AnyAsync(l => l.Id == libraryId);
        if (!libraryExists)
            return null;

        return await context.Books.Where(b => b.LibraryId == libraryId).ToListAsync();
    }
}
