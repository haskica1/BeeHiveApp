using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeeHive.Entity.Configurations;

public class LearningTopicReadConfiguration : IEntityTypeConfiguration<LearningTopicRead>
{
    public void Configure(EntityTypeBuilder<LearningTopicRead> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.TopicId, r.UserId }).IsUnique();

        builder.ToTable("LearningTopicReads");
    }
}
