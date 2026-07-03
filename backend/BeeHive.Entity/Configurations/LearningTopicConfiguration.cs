using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeHive.Entity.Configurations;

public class LearningTopicConfiguration : IEntityTypeConfiguration<LearningTopic>
{
    public void Configure(EntityTypeBuilder<LearningTopic> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Category).IsRequired();
        builder.Property(t => t.Summary).IsRequired().HasMaxLength(300);
        builder.Property(t => t.BodyMarkdown).IsRequired();
        builder.Property(t => t.Months); // Npgsql: integer[], nullable = evergreen

        builder.HasMany(t => t.Reads)
            .WithOne(r => r.Topic)
            .HasForeignKey(r => r.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.IsPublished);
        builder.HasIndex(t => t.Category);

        builder.ToTable("LearningTopics");
    }
}
