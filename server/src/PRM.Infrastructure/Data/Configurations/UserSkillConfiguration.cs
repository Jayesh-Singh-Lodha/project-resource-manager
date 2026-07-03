using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRM.Core.Entities;

namespace PRM.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for the UserSkills join table.
/// Composite PK: (UserId, SkillId).
/// </summary>
public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("user_skills");

        builder.HasKey(us => new { us.UserId, us.SkillId });

        builder.Property(us => us.UserId)
            .HasColumnName("user_id");

        builder.Property(us => us.SkillId)
            .HasColumnName("skill_id");

        builder.Property(us => us.ProficiencyLevel)
            .HasColumnName("proficiency_level")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(us => us.User)
            .WithMany(u => u.UserSkills)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(us => us.Skill)
            .WithMany(s => s.UserSkills)
            .HasForeignKey(us => us.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
