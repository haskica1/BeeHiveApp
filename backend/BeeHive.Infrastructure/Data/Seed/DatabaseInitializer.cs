using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Infrastructure.Data.Seed;

/// <summary>
/// Runs after migrations to ensure seed users exist with properly hashed passwords.
/// Safe to call on every startup — checks before inserting.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task SeedUsersAsync(BeeHiveDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var orgs = await context.Organizations.ToListAsync();
        var goldenHive = orgs.First(o => o.Id == 1);
        var mountainBees = orgs.First(o => o.Id == 2);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        context.Users.AddRange(
            new User
            {
                FirstName = "Alice",
                LastName = "Goldsworth",
                Email = "admin@goldenhive.com",
                PasswordHash = passwordHash,
                Role = UserRole.Admin,
                OrganizationId = goldenHive.Id,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FirstName = "Marco",
                LastName = "Bianchi",
                Email = "admin@mountainbees.com",
                PasswordHash = passwordHash,
                Role = UserRole.Admin,
                OrganizationId = mountainBees.Id,
                CreatedAt = DateTime.UtcNow
            }
        );

        await context.SaveChangesAsync();
    }
}
