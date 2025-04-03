using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Page> Pages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the self-referencing relationship
        modelBuilder
            .Entity<Page>()
            .HasOne(p => p.ParentPage) // A page has one parent
            .WithMany(p => p.ChildPages) // A parent can have many children
            .HasForeignKey(p => p.ParentPageId) // The foreign key is ParentPageId
            .OnDelete(DeleteBehavior.Restrict); // Or Cascade, SetNull, etc. depending on desired behavior when a parent is deleted

        // Seed initial data for pages
        modelBuilder
            .Entity<Page>()
            .HasData(
                new Page
                {
                    Id = 1,
                    Title = "Politik 101",
                    Content = "# Politik 101\n\nPolitik handler om...",
                    ParentPageId = null,
                    DisplayOrder = 1,
                },
                new Page
                {
                    Id = 2,
                    Title = "Den Politiske Akse",
                    Content = "## Den Politiske Akse...",
                    ParentPageId = 1,
                    DisplayOrder = 1,
                },
                new Page
                {
                    Id = 3,
                    Title = "Venstre vs Højre",
                    Content = "### Venstre vs Højre...",
                    ParentPageId = 2,
                    DisplayOrder = 1,
                },
                new Page
                {
                    Id = 4,
                    Title = "Højre",
                    Content = "# Højre \n\n Højre er at være højre...",
                    ParentPageId = 3,
                    DisplayOrder = 1,
                },
                new Page
                {
                    Id = 5,
                    Title = "Venstre",
                    Content = "# Venstre \n\n Venstre er at være venstre...",
                    ParentPageId = 3,
                    DisplayOrder = 2,
                }
            );
    }
}
