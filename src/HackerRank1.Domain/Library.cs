using System.ComponentModel.DataAnnotations;

namespace HackerRank1.Domain;

public class Library
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}
