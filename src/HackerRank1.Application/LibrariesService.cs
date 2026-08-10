using HackerRank1.Domain;

namespace HackerRank1.Application;

public interface ILibrariesService
{
    Task<IEnumerable<Library>> Get(int[]? ids);

    Task<Library?> GetById(int id);

    Task<Library> Add(Library library);

    Task<Library> Update(Library library);

    Task<bool> Delete(int id);
}

public class LibrariesService : ILibrariesService
{
    private readonly ILibraryRepository _libraryRepository;

    public LibrariesService(ILibraryRepository libraryRepository)
    {
        _libraryRepository = libraryRepository;
    }

    public Task<IEnumerable<Library>> Get(int[]? ids)
        => _libraryRepository.GetAsync(ids);

    public Task<Library?> GetById(int id)
        => _libraryRepository.GetByIdAsync(id);

    public Task<Library> Add(Library library)
        => _libraryRepository.AddAsync(library);

    public Task<Library> Update(Library library)
        => _libraryRepository.UpdateAsync(library);

    public Task<bool> Delete(int id)
        => _libraryRepository.DeleteAsync(id);
}
