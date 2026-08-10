using HackerRank1.API.DTO;
using HackerRank1.Application;
using HackerRank1.Domain;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.API.Controllers;

[ApiController]
[Route("api/libraries/{libraryId}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBooksService _booksService;
    private readonly ILibrariesService _librariesService;

    public BooksController(IBooksService booksService, ILibrariesService librariesService)
    {
        _booksService = booksService;
        _librariesService = librariesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int libraryId)
    {
        var library = await _librariesService.GetById(libraryId);
        if (library == null)
            return NotFound();

        var books = await _booksService.Get(libraryId, null);
        return Ok(books);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int libraryId, BookForm form)
    {
        var library = await _librariesService.GetById(libraryId);
        if (library == null)
            return NotFound();

        var created = await _booksService.Add(new Book { Name = form.Name, Category = form.Category, LibraryId = libraryId });
        return CreatedAtAction(nameof(GetAll), new { libraryId }, created);
    }

    [HttpPut("{bookId}")]
    public async Task<IActionResult> Update(int libraryId, int bookId, BookForm form)
    {
        var book = await _booksService.GetById(bookId);
        if (book == null || book.LibraryId != libraryId)
            return NotFound();

        book.Name = form.Name;
        book.Category = form.Category;
        await _booksService.Update(book);
        return NoContent();
    }

    [HttpDelete("{bookId}")]
    public async Task<IActionResult> Delete(int libraryId, int bookId)
    {
        var book = await _booksService.GetById(bookId);
        if (book == null || book.LibraryId != libraryId)
            return NotFound();

        var deleted = await _booksService.Delete(bookId);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
