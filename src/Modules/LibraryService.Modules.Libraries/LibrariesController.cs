using LibraryService.Modules.Libraries.Features;
using LibraryService.SharedKernel.Data;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Modules.Libraries;

[ApiController]
[Route("api/[controller]")]
public class LibrariesController : ControllerBase
{
    private readonly LibraryContext _context;

    public LibrariesController(LibraryContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var libraries = await ListLibraries.Query(_context);
        return Ok(libraries);
    }

    [HttpGet("{libraryId}")]
    public async Task<IActionResult> Get(int libraryId)
    {
        var library = await GetLibrary.Query(_context, libraryId);
        if (library is null)
            return NotFound();

        return Ok(library);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Library library)
    {
        var created = await CreateLibrary.Execute(_context, library);
        return Ok(created);
    }

    [HttpPut("{libraryId}")]
    public async Task<IActionResult> Update(int libraryId, Library library)
    {
        var updated = await UpdateLibrary.Execute(_context, libraryId, library);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{libraryId}")]
    public async Task<IActionResult> Delete(int libraryId)
    {
        var deleted = await DeleteLibrary.Execute(_context, libraryId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
