using LibraryService.SharedKernel.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Modules.Books.Features;

public static class CreateBook
{
    public static async Task<Book?> Execute(LibraryContext context, int libraryId, BookForm form)
    {
        var libraryExists = await context.Libraries.AnyAsync(l => l.Id == libraryId);
        if (!libraryExists)
            return null;

        var book = new Book
        {
            Name = form.Name ?? string.Empty,
            Category = form.Category ?? string.Empty,
            LibraryId = libraryId
        };

        context.Books.Add(book);
        await context.SaveChangesAsync();
        return book;
    }
}
