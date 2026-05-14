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
        // Fix any Admin users that were seeded without an ApiaryId (e.g. from an older
        // version of this seeder). Assign the first apiary in their organisation.
        var adminsWithoutApiary = await context.Users
            .Where(u => u.Role == UserRole.Admin && u.ApiaryId == null && u.OrganizationId != null)
            .ToListAsync();

        bool anyFixed = false;
        foreach (var admin in adminsWithoutApiary)
        {
            var apiary = await context.Apiaries
                .Where(a => a.OrganizationId == admin.OrganizationId)
                .OrderBy(a => a.Id)
                .FirstOrDefaultAsync();

            if (apiary == null) continue;
            admin.ApiaryId = apiary.Id;
            anyFixed = true;
        }

        if (anyFixed)
            await context.SaveChangesAsync();

        var orgs = await context.Organizations.ToListAsync();
        var goldenHive   = orgs.FirstOrDefault(o => o.Id == 1);
        var mountainBees = orgs.FirstOrDefault(o => o.Id == 2);

        // Resolve actual apiary IDs from the database rather than hardcoding them,
        // so the seeder survives if the seeded apiaries were deleted or IDs shifted.
        var goldenHiveApiaryId = goldenHive == null ? null :
            await context.Apiaries
                .Where(a => a.OrganizationId == goldenHive.Id)
                .OrderBy(a => a.Id)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();

        var mountainBeesApiaryId = mountainBees == null ? null :
            await context.Apiaries
                .Where(a => a.OrganizationId == mountainBees.Id)
                .OrderBy(a => a.Id)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();

        var usersToAdd = new List<User>();

        // ── SystemAdmin ───────────────────────────────────────────────────────
        // Two system-wide admins with no org affiliation.

        AddIfMissing(context, usersToAdd, new User
        {
            FirstName    = "System",
            LastName     = "Admin",
            Email        = "sysadmin@beehive.com",
            PasswordHash = Hash("SysAdmin123!"),
            Role         = UserRole.SystemAdmin,
            CreatedAt    = DateTime.UtcNow
        });

        AddIfMissing(context, usersToAdd, new User
        {
            FirstName    = "Emma",
            LastName     = "Systems",
            Email        = "sysadmin2@beehive.com",
            PasswordHash = Hash("SysAdmin123!"),
            Role         = UserRole.SystemAdmin,
            CreatedAt    = DateTime.UtcNow
        });

        // ── Admin ─────────────────────────────────────────────────────────────
        // Apiary-scoped admins — one per apiary per org.
        // ApiaryId is looked up at runtime so hardcoded IDs never cause FK failures.

        if (goldenHive != null && goldenHiveApiaryId != null)
        {
            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Alice",
                LastName       = "Goldsworth",
                Email          = "admin@goldenhive.com",
                PasswordHash   = Hash("Admin123!"),
                Role           = UserRole.Admin,
                OrganizationId = goldenHive.Id,
                ApiaryId       = goldenHiveApiaryId,
                CreatedAt      = DateTime.UtcNow
            });

            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "James",
                LastName       = "Holt",
                Email          = "admin2@goldenhive.com",
                PasswordHash   = Hash("Admin123!"),
                Role           = UserRole.Admin,
                OrganizationId = goldenHive.Id,
                ApiaryId       = goldenHiveApiaryId,
                CreatedAt      = DateTime.UtcNow
            });
        }

        if (mountainBees != null && mountainBeesApiaryId != null)
        {
            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Marco",
                LastName       = "Bianchi",
                Email          = "admin@mountainbees.com",
                PasswordHash   = Hash("Admin123!"),
                Role           = UserRole.Admin,
                OrganizationId = mountainBees.Id,
                ApiaryId       = mountainBeesApiaryId,
                CreatedAt      = DateTime.UtcNow
            });

            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Nina",
                LastName       = "Horvat",
                Email          = "admin2@mountainbees.com",
                PasswordHash   = Hash("Admin123!"),
                Role           = UserRole.Admin,
                OrganizationId = mountainBees.Id,
                ApiaryId       = mountainBeesApiaryId,
                CreatedAt      = DateTime.UtcNow
            });
        }

        // ── OrgAdmin ──────────────────────────────────────────────────────────
        // Org-level admins — manage the entire organisation.

        if (goldenHive != null)
        {
            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Bob",
                LastName       = "Keeper",
                Email          = "orgadmin@goldenhive.com",
                PasswordHash   = Hash("OrgAdmin123!"),
                Role           = UserRole.OrgAdmin,
                OrganizationId = goldenHive.Id,
                CreatedAt      = DateTime.UtcNow
            });

            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Diana",
                LastName       = "Fields",
                Email          = "orgadmin2@goldenhive.com",
                PasswordHash   = Hash("OrgAdmin123!"),
                Role           = UserRole.OrgAdmin,
                OrganizationId = goldenHive.Id,
                CreatedAt      = DateTime.UtcNow
            });
        }

        if (mountainBees != null)
        {
            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Sofia",
                LastName       = "Petrović",
                Email          = "orgadmin@mountainbees.com",
                PasswordHash   = Hash("OrgAdmin123!"),
                Role           = UserRole.OrgAdmin,
                OrganizationId = mountainBees.Id,
                CreatedAt      = DateTime.UtcNow
            });

            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Luka",
                LastName       = "Novak",
                Email          = "orgadmin2@mountainbees.com",
                PasswordHash   = Hash("OrgAdmin123!"),
                Role           = UserRole.OrgAdmin,
                OrganizationId = mountainBees.Id,
                CreatedAt      = DateTime.UtcNow
            });
        }

        // ── User ──────────────────────────────────────────────────────────────
        // Regular beekeepers — two per organisation.

        if (goldenHive != null)
        {
            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Tom",
                LastName       = "Meadows",
                Email          = "user1@goldenhive.com",
                PasswordHash   = Hash("User123!"),
                Role           = UserRole.User,
                OrganizationId = goldenHive.Id,
                CreatedAt      = DateTime.UtcNow
            });

            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Laura",
                LastName       = "Bloom",
                Email          = "user2@goldenhive.com",
                PasswordHash   = Hash("User123!"),
                Role           = UserRole.User,
                OrganizationId = goldenHive.Id,
                CreatedAt      = DateTime.UtcNow
            });
        }

        if (mountainBees != null)
        {
            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Ivan",
                LastName       = "Petrov",
                Email          = "user1@mountainbees.com",
                PasswordHash   = Hash("User123!"),
                Role           = UserRole.User,
                OrganizationId = mountainBees.Id,
                CreatedAt      = DateTime.UtcNow
            });

            AddIfMissing(context, usersToAdd, new User
            {
                FirstName      = "Ana",
                LastName       = "Kovač",
                Email          = "user2@mountainbees.com",
                PasswordHash   = Hash("User123!"),
                Role           = UserRole.User,
                OrganizationId = mountainBees.Id,
                CreatedAt      = DateTime.UtcNow
            });
        }

        if (usersToAdd.Count > 0)
        {
            context.Users.AddRange(usersToAdd);
            await context.SaveChangesAsync();
        }
    }

    private static void AddIfMissing(BeeHiveDbContext context, List<User> batch, User user)
    {
        if (!context.Users.Any(u => u.Email == user.Email) &&
            batch.All(u => u.Email != user.Email))
            batch.Add(user);
    }

    private static string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);
}
