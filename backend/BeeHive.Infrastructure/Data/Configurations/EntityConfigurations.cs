using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeHive.Infrastructure.Data.Configurations;

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

        // One apiary → many beehives; cascade delete removes beehives when apiary is deleted
        builder.HasMany(a => a.Beehives)
            .WithOne(b => b.Apiary)
            .HasForeignKey(b => b.ApiaryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Apiaries");
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

        // Store enum as integer for performance; consider string if human-readable DB matters
        builder.Property(b => b.Type)
            .IsRequired();

        builder.Property(b => b.Material)
            .IsRequired();

        // One beehive → many inspections; cascade delete
        builder.HasMany(b => b.Inspections)
            .WithOne(i => i.Beehive)
            .HasForeignKey(i => i.BeehiveId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Beehives");
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
