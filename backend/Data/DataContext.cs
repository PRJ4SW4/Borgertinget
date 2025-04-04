using Microsoft.EntityFrameworkCore;
using backend.Models;
using BCrypt.Net;

namespace backend.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Hardcoded user for testing purposes
        var id = 1;
        var email = "dev@testing.com";
        var username = "dev";
        var password = BCrypt.Net.BCrypt.HashPassword("Dev12345");

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = id,
            Email = email,
            UserName = username,
            PasswordHash = password,
            IsVerified = true,
            VerificationToken = null
        });
    }
}
