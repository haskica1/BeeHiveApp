using BeeHive.Domain.Entities;
using BeeHive.Infrastructure.Data.Configurations;
using BeeHive.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Infrastructure.Data;

/// <summary>
/// Main EF Core database context for the BeeHive application.
/// Each DbSet corresponds to a database table managed by EF Core migrations.
/// </summary>
public class BeeHiveDbContext : DbContext
{
    public BeeHiveDbContext(DbContextOptions<BeeHiveDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Apiary> Apiaries => Set<Apiary>();
    public DbSet<Beehive> Beehives => Set<Beehive>();
    public DbSet<UserBeehive> UserBeehives => Set<UserBeehive>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<Diet> Diets => Set<Diet>();
    public DbSet<FeedingEntry> FeedingEntries => Set<FeedingEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BeeHiveDbContext).Assembly);

        // Seed initial data
        DataSeeder.Seed(modelBuilder);
    }

    /// <summary>
    /// Automatically sets UpdatedAt on modified entities before saving, and normalises
    /// any DateTime.Unspecified values to UTC so Npgsql accepts them as timestamptz.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            if (entry.State == EntityState.Modified &&
                entry.Entity is BeeHive.Domain.Common.BaseEntity entity)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }

            // Npgsql 6+ requires DateTimeKind.Utc for timestamptz columns.
            // User-supplied dates arrive as Kind=Unspecified; convert them here
            // so callers don't need to remember to do it everywhere.
            foreach (var prop in entry.Properties)
            {
                if (prop.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                    prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
