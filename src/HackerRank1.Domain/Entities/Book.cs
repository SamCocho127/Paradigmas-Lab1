namespace HackerRank1.Domain.Entities;

public class Book
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Category { get; set; }

    public int LibraryId { get; set; }

    public virtual Library Library { get; set; }
}
