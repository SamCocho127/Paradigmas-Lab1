using LibraryService.SharedKernel.Data;

namespace LibraryService.Modules.Libraries.Features;

public static class CreateLibrary
{
    public static async Task<Library> Execute(LibraryContext context, Library library)
    {
        await context.Libraries.AddAsync(library);
        await context.SaveChangesAsync();
        return library;
    }
}
