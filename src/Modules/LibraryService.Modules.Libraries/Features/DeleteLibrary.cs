using LibraryService.SharedKernel.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Modules.Libraries.Features;

public static class DeleteLibrary
{
    public static async Task<bool> Execute(LibraryContext context, int id)
    {
        var existing = await context.Libraries.SingleOrDefaultAsync(l => l.Id == id);
        if (existing is null)
            return false;

        var books = await context.Books.Where(b => b.LibraryId == id).ToListAsync();
        context.Books.RemoveRange(books);
        context.Libraries.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}
