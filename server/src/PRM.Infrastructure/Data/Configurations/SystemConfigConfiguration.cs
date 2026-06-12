using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRM.Core.Entities;

namespace PRM.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for the SystemConfig table.
/// PK is the config key (string) — no auto-increment.
/// </summary>
public class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
{
    public void Configure(EntityTypeBuilder<SystemConfig> builder)
    {
        builder.ToTable("system_config");

        builder.HasKey(sc => sc.Key);

        builder.Property(sc => sc.Key)
            .HasColumnName("config_key")
            .HasMaxLength(100);

        builder.Property(sc => sc.Value)
            .HasColumnName("config_value")
            .HasMaxLength(500)
            .IsRequired();
    }
}
