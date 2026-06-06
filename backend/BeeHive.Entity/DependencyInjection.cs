using BeeHive.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeeHive.Entity;

/// <summary>
/// Registers the persistence layer: the EF Core <see cref="BeeHiveDbContext"/>, the Unit of Work,
/// and (transitively) all repositories. The data project owns the database connection and migrations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddEntity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BeeHiveDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(BeeHiveDbContext).Assembly.FullName)
            ));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
