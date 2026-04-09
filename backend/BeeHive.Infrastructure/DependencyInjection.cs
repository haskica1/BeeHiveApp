using BeeHive.Application.Common.Interfaces;
using BeeHive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeeHive.Infrastructure;

/// <summary>
/// Registers all Infrastructure-layer services (EF Core, repositories, UoW).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BeeHiveDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(BeeHiveDbContext).Assembly.FullName)
            ));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
