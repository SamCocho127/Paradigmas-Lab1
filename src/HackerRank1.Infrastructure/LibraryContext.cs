using HackerRank1.Domain;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Infrastructure;

public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    { }

    public DbSet<Library> Libraries { get; set; } = null!;

    public DbSet<Book> Books { get; set; } = null!;
}
