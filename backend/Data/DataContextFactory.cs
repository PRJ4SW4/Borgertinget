using System.IO;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace backend.Data
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            DotNetEnv.Env.Load(envPath); // Prøv evt. bare Env.Load() hvis .env er i samme mappe som .csproj

            // Bygger konfigurationen ud fra appsettings.json
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var builder = new DbContextOptionsBuilder<DataContext>();
            builder.UseNpgsql(connectionString);

            return new DataContext(builder.Options);
        }
    }
}
