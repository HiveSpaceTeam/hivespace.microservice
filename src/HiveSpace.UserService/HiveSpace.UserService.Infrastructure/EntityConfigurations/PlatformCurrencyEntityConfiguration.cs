using HiveSpace.UserService.Domain.Aggregates.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.UserService.Infrastructure.EntityConfigurations;

public class PlatformCurrencyEntityConfiguration : IEntityTypeConfiguration<PlatformCurrency>
{
    public void Configure(EntityTypeBuilder<PlatformCurrency> builder)
    {
        builder.ToTable("platform_currencies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConfigType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.HasIndex(x => new { x.ConfigType, x.CurrencyCode }).IsUnique();
    }
}
