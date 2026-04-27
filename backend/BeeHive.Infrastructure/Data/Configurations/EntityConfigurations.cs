using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeHive.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .HasMaxLength(1000);

        builder.HasOne(o => o.CreatedBy)
            .WithMany()
            .HasForeignKey(o => o.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasMany(o => o.Users)
            .WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Apiaries)
            .WithOne(a => a.Organization)
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Organizations");
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired();

        // Admin-level users are scoped to a single apiary
        builder.HasOne(u => u.Apiary)
            .WithMany()
            .HasForeignKey(u => u.ApiaryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.ToTable("Users");
    }
}

public class ApiaryConfiguration : IEntityTypeConfiguration<Apiary>
{
    public void Configure(EntityTypeBuilder<Apiary> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.Latitude)
            .HasColumnType("double precision");

        builder.Property(a => a.Longitude)
            .HasColumnType("double precision");

        builder.HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // One apiary → many beehives; cascade delete removes beehives when apiary is deleted
        builder.HasMany(a => a.Beehives)
            .WithOne(b => b.Apiary)
            .HasForeignKey(b => b.ApiaryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Apiaries");
    }
}

public class UserBeehiveConfiguration : IEntityTypeConfiguration<UserBeehive>
{
    public void Configure(EntityTypeBuilder<UserBeehive> builder)
    {
        builder.HasKey(ub => new { ub.UserId, ub.BeehiveId });

        builder.HasOne(ub => ub.User)
            .WithMany(u => u.AssignedBeehives)
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ub => ub.Beehive)
            .WithMany(b => b.AssignedUsers)
            .HasForeignKey(ub => ub.BeehiveId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("UserBeehives");
    }
}

public class BeehiveConfiguration : IEntityTypeConfiguration<Beehive>
{
    public void Configure(EntityTypeBuilder<Beehive> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Notes)
            .HasMaxLength(2000);

        builder.Property(b => b.UniqueId)
            .HasColumnType("uuid");

        // Enforce uniqueness on UniqueId (non-null rows only, so nullable GUIDs are still allowed)
        builder.HasIndex(b => b.UniqueId)
            .IsUnique()
            .HasFilter("\"UniqueId\" IS NOT NULL");

        // Store enum as integer for performance; consider string if human-readable DB matters
        builder.Property(b => b.Type)
            .IsRequired();

        builder.Property(b => b.Material)
            .IsRequired();

        builder.HasOne(b => b.CreatedBy)
            .WithMany()
            .HasForeignKey(b => b.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // One beehive → many inspections; cascade delete
        builder.HasMany(b => b.Inspections)
            .WithOne(i => i.Beehive)
            .HasForeignKey(i => i.BeehiveId)
            .OnDelete(DeleteBehavior.Cascade);

        // One beehive → many diets; cascade delete
        builder.HasMany(b => b.Diets)
            .WithOne(d => d.Beehive)
            .HasForeignKey(d => d.BeehiveId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Beehives");
    }
}

public class DietConfiguration : IEntityTypeConfiguration<Diet>
{
    public void Configure(EntityTypeBuilder<Diet> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.CustomReason)
            .HasMaxLength(500);

        builder.Property(d => d.CustomFoodType)
            .HasMaxLength(200);

        builder.Property(d => d.EarlyCompletionComment)
            .HasMaxLength(1000);

        builder.Property(d => d.Reason).IsRequired();
        builder.Property(d => d.FoodType).IsRequired();
        builder.Property(d => d.Status).IsRequired();
        builder.Property(d => d.StartDate).IsRequired();

        builder.HasOne(d => d.CreatedBy)
            .WithMany()
            .HasForeignKey(d => d.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // One diet → many feeding entries; cascade delete
        builder.HasMany(d => d.FeedingEntries)
            .WithOne(e => e.Diet)
            .HasForeignKey(e => e.DietId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.BeehiveId);

        builder.ToTable("Diets");
    }
}

public class FeedingEntryConfiguration : IEntityTypeConfiguration<FeedingEntry>
{
    public void Configure(EntityTypeBuilder<FeedingEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ScheduledDate).IsRequired();
        builder.Property(e => e.Status).IsRequired();

        builder.HasIndex(e => e.DietId);
        builder.HasIndex(e => e.ScheduledDate);

        builder.ToTable("FeedingEntries");
    }
}

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Date)
            .IsRequired();

        builder.Property(i => i.HoneyLevel)
            .IsRequired();

        builder.Property(i => i.BroodStatus)
            .HasMaxLength(500);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        // Index on Date for common time-range queries
        builder.HasIndex(i => i.Date);
        builder.HasIndex(i => i.BeehiveId);

        builder.ToTable("Inspections");
    }
}

public class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasDefaultValue(TodoPriority.Medium);

        builder.Property(t => t.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // NoAction to avoid multiple cascade paths from Users (CreatedById already uses SetNull).
        // AssignedToId must be cleared in the service before a user can be deleted.
        builder.HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        // Optional FK to Apiary — NoAction to avoid multiple cascade paths from Apiary.
        // Apiary-level todos are deleted explicitly in ApiaryService.DeleteAsync.
        builder.HasOne(t => t.Apiary)
            .WithMany()
            .HasForeignKey(t => t.ApiaryId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        // Optional FK to Beehive
        builder.HasOne(t => t.Beehive)
            .WithMany()
            .HasForeignKey(t => t.BeehiveId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasIndex(t => t.ApiaryId);
        builder.HasIndex(t => t.BeehiveId);

        builder.ToTable("Todos");
    }
}
