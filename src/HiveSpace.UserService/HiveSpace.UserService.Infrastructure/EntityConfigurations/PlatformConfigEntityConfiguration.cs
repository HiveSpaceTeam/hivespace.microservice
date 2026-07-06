using HiveSpace.UserService.Domain.Aggregates.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.UserService.Infrastructure.EntityConfigurations;

public class PlatformConfigEntityConfiguration : IEntityTypeConfiguration<PlatformConfig>
{
    public void Configure(EntityTypeBuilder<PlatformConfig> builder)
    {
        builder.ToTable("platform_configs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConfigType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DefaultCurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.HasIndex(x => x.ConfigType).IsUnique();
    }
}
