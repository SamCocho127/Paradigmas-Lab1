using LibraryService.SharedKernel.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Modules.Libraries.Features;

public static class UpdateLibrary
{
    public static async Task<bool> Execute(LibraryContext context, int id, Library library)
    {
        var existing = await context.Libraries.SingleOrDefaultAsync(l => l.Id == id);
        if (existing is null)
            return false;

        existing.Name = library.Name ?? string.Empty;
        existing.Location = library.Location ?? string.Empty;
        await context.SaveChangesAsync();
        return true;
    }
}
