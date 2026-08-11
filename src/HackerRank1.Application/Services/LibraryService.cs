using HackerRank1.Application.Interfaces;
using HackerRank1.Domain.Entities;

namespace HackerRank1.Application.Services;

public class LibrariesService : ILibrariesService
{
    private readonly ILibraryRepository _libraryRepository;

    public LibrariesService(ILibraryRepository libraryRepository)
    {
        _libraryRepository = libraryRepository;
    }

    public async Task<IEnumerable<Library>> Get(int[] ids)
    {
        return await _libraryRepository.ListAsync(ids);
    }

    public async Task<Library> Add(Library library)
    {
        await _libraryRepository.AddAsync(library);
        await _libraryRepository.SaveChangesAsync();
        return library;
    }

    public async Task<Library> Update(Library library)
    {
        var projectForChanges = await _libraryRepository.GetByIdAsync(library.Id);
        projectForChanges.Name = library.Name;
        projectForChanges.Location = library.Location;

        await _libraryRepository.UpdateAsync(projectForChanges);
        await _libraryRepository.SaveChangesAsync();
        return library;
    }

    public async Task<bool> Delete(Library library)
    {
        // Complete the implementation
        throw new NotImplementedException();
    }
}

public interface ILibrariesService
{
    Task<IEnumerable<Library>> Get(int[] ids);

    Task<Library> Add(Library library);

    Task<Library> Update(Library library);

    Task<bool> Delete(Library library);
}
