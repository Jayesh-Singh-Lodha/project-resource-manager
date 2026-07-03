using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRM.Core.Entities;

namespace PRM.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for the Timesheets table.
/// </summary>
public class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
{
    public void Configure(EntityTypeBuilder<Timesheet> builder)
    {
        builder.ToTable("timesheets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("timesheet_id");

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.WeekStartDate)
            .HasColumnName("week_start_date")
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.SubmittedAt)
            .HasColumnName("submitted_at");

        builder.HasOne(t => t.User)
            .WithMany(u => u.Timesheets)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Business rule: one timesheet per user per week
        builder.HasIndex(t => new { t.UserId, t.WeekStartDate })
            .IsUnique()
            .HasDatabaseName("ix_timesheets_user_week");
    }
}
