using LibraryService.Modules.Books.Features;
using LibraryService.SharedKernel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Modules.Books;

[ApiController]
[Route("api/libraries/{libraryId}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly LibraryContext _context;

    public BooksController(LibraryContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(int libraryId)
    {
        var books = await ListBooks.Query(_context, libraryId);
        if (books is null)
            return NotFound();

        return Ok(books);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add(int libraryId, BookForm form)
    {
        var book = await CreateBook.Execute(_context, libraryId, form);
        if (book is null)
            return NotFound();

        return CreatedAtAction(nameof(GetAll), new { libraryId }, book);
    }
}
