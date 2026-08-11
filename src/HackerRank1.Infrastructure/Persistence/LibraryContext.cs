using HackerRank1.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Infrastructure.Persistence;

public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    { }

    public DbSet<Library> Libraries { get; set; }

    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Library>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).IsRequired();
            entity.Property(l => l.Location).IsRequired();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired();
            entity.Property(b => b.Category).IsRequired();
            entity.HasOne(b => b.Library)
                .WithMany()
                .HasForeignKey(b => b.LibraryId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }
}
