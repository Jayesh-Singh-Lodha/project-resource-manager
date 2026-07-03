using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRM.Core.Entities;
using PRM.Core.Enums;

namespace PRM.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for the Projects table.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("project_id");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(p => p.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.ManagerId)
            .HasColumnName("manager_id");

        builder.Property(p => p.TotalStoryPoints)
            .HasColumnName("total_story_points")
            .IsRequired();

        builder.Property(p => p.HealthStatus)
            .HasColumnName("health_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(HealthStatus.OnTrack)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // FK: Project.Manager → User
        builder.HasOne(p => p.Manager)
            .WithMany(u => u.ManagedProjects)
            .HasForeignKey(p => p.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
