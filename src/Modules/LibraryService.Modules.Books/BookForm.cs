namespace LibraryService.Modules.Books;

public class BookForm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int LibraryId { get; set; }
}
