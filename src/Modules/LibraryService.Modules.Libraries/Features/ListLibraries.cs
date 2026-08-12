using LibraryService.SharedKernel.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Modules.Libraries.Features;

public static class ListLibraries
{
    public static async Task<IEnumerable<Library>> Query(LibraryContext context)
        => await context.Libraries.ToListAsync();
}
