using HackerRank1.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.API.Controllers;

[ApiController]
[Route("api/libraries/{libraryId}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ILibrariesService _librariesService;
    private readonly IBooksService _booksService;

    public BooksController(IBooksService booksService, ILibrariesService librariesService)
    {
        _librariesService = librariesService;
        _booksService = booksService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(int libraryId)
    {
        var books = await _booksService.Get(libraryId, null);
        return Ok(books);
    }
}
