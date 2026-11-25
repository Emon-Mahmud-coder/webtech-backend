using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Book>().HasIndex(x => x.Isbn).IsUnique();
        b.Entity<Book>().Property(x => x.Price).HasPrecision(10, 2);
        b.Entity<Author>().HasMany(a => a.Books).WithOne(x => x.Author)
            .HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}
