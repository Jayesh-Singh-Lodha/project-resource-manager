using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRM.Core.Entities;

namespace PRM.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for the Allocations table.
/// </summary>
public class AllocationConfiguration : IEntityTypeConfiguration<Allocation>
{
    public void Configure(EntityTypeBuilder<Allocation> builder)
    {
        builder.ToTable("allocations");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("allocation_id");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(a => a.UtilisationPercent)
            .HasColumnName("utilisation_percent")
            .IsRequired();

        builder.Property(a => a.FromDate)
            .HasColumnName("from_date")
            .IsRequired();

        builder.Property(a => a.ToDate)
            .HasColumnName("to_date")
            .IsRequired();

        builder.HasOne(a => a.User)
            .WithMany(u => u.Allocations)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Project)
            .WithMany(p => p.Allocations)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for querying allocations by user
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_allocations_user_id");

        // Index for querying allocations by project
        builder.HasIndex(a => a.ProjectId)
            .HasDatabaseName("ix_allocations_project_id");
    }
}
