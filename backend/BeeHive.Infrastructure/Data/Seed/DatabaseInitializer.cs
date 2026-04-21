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
        var orgs = await context.Organizations.ToListAsync();
        var goldenHive = orgs.FirstOrDefault(o => o.Id == 1);
        var mountainBees = orgs.FirstOrDefault(o => o.Id == 2);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        var usersToAdd = new List<User>();

        if (!await context.Users.AnyAsync(u => u.Email == "sysadmin@beehive.com"))
            usersToAdd.Add(new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = "sysadmin@beehive.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SysAdmin123!"),
                Role = UserRole.SystemAdmin,
                OrganizationId = null,
                CreatedAt = DateTime.UtcNow
            });

        if (goldenHive != null && !await context.Users.AnyAsync(u => u.Email == "admin@goldenhive.com"))
            usersToAdd.Add(new User
            {
                FirstName = "Alice",
                LastName = "Goldsworth",
                Email = "admin@goldenhive.com",
                PasswordHash = passwordHash,
                Role = UserRole.Admin,
                OrganizationId = goldenHive.Id,
                CreatedAt = DateTime.UtcNow
            });

        if (mountainBees != null && !await context.Users.AnyAsync(u => u.Email == "admin@mountainbees.com"))
            usersToAdd.Add(new User
            {
                FirstName = "Marco",
                LastName = "Bianchi",
                Email = "admin@mountainbees.com",
                PasswordHash = passwordHash,
                Role = UserRole.Admin,
                OrganizationId = mountainBees.Id,
                CreatedAt = DateTime.UtcNow
            });

        if (usersToAdd.Count > 0)
        {
            context.Users.AddRange(usersToAdd);
            await context.SaveChangesAsync();
        }
    }
}
