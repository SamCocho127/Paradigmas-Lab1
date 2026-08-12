using LibraryService.SharedKernel.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Modules.Libraries.Features;

public static class GetLibrary
{
    public static async Task<Library?> Query(LibraryContext context, int id)
        => await context.Libraries.SingleOrDefaultAsync(l => l.Id == id);
}
