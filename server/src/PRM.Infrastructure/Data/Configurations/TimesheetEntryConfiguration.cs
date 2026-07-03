using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRM.Core.Entities;

namespace PRM.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for the TimesheetEntries table.
/// </summary>
public class TimesheetEntryConfiguration : IEntityTypeConfiguration<TimesheetEntry>
{
    public void Configure(EntityTypeBuilder<TimesheetEntry> builder)
    {
        builder.ToTable("timesheet_entries");

        builder.HasKey(te => te.Id);

        builder.Property(te => te.Id)
            .HasColumnName("entry_id");

        builder.Property(te => te.TimesheetId)
            .HasColumnName("timesheet_id")
            .IsRequired();

        builder.Property(te => te.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(te => te.HoursWorked)
            .HasColumnName("hours_worked")
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(te => te.ActivityTags)
            .HasColumnName("activity_tags")
            .HasMaxLength(500);

        builder.HasOne(te => te.Timesheet)
            .WithMany(t => t.Entries)
            .HasForeignKey(te => te.TimesheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(te => te.Project)
            .WithMany()
            .HasForeignKey(te => te.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
