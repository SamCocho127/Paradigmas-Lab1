using HackerRank1.API.DTO;
using HackerRank1.Application;
using HackerRank1.Domain;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LibrariesController : ControllerBase
{
    private readonly ILibrariesService _librariesService;

    public LibrariesController(ILibrariesService librariesService)
    {
        _librariesService = librariesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var libraries = await _librariesService.Get(null);
        return Ok(libraries);
    }

    [HttpGet("{libraryId}")]
    public async Task<IActionResult> Get(int libraryId)
    {
        var library = await _librariesService.GetById(libraryId);
        if (library == null)
            return NotFound();
        return Ok(library);
    }

    [HttpPost]
    public async Task<IActionResult> Add(LibraryForm form)
    {
        var created = await _librariesService.Add(new Library { Name = form.Name, Location = form.Location });
        return CreatedAtAction(nameof(Get), new { libraryId = created.Id }, created);
    }

    [HttpPut("{libraryId}")]
    public async Task<IActionResult> Update(int libraryId, LibraryForm form)
    {
        var existing = await _librariesService.GetById(libraryId);
        if (existing == null)
            return NotFound();

        await _librariesService.Update(new Library { Id = libraryId, Name = form.Name, Location = form.Location });
        return NoContent();
    }

    [HttpDelete("{libraryId}")]
    public async Task<IActionResult> Delete(int libraryId)
    {
        var deleted = await _librariesService.Delete(libraryId);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
