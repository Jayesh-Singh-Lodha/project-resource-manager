using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRM.Core.Entities;

namespace PRM.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for the Milestones table.
/// </summary>
public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("milestones");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("milestone_id");

        builder.Property(m => m.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(m => m.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(m => m.StoryPoints)
            .HasColumnName("story_points")
            .IsRequired();

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
