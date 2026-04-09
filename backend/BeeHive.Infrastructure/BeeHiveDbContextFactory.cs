using BeeHive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace BeeHive.Infrastructure;

/// <summary>
/// Used exclusively by the EF Core CLI tools (dotnet ef migrations add, database update, etc.)
/// at design time. Not used at runtime — the real DbContext is registered via DependencyInjection.cs.
/// </summary>
public class BeeHiveDbContextFactory : IDesignTimeDbContextFactory<BeeHiveDbContext>
{
    public BeeHiveDbContext CreateDbContext(string[] args)
    {
        // Walk up from Infrastructure project to find appsettings.json in the API project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../BeeHive.API");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(basePath, "appsettings.Development.json"), optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<BeeHiveDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new BeeHiveDbContext(optionsBuilder.Options);
    }
}
