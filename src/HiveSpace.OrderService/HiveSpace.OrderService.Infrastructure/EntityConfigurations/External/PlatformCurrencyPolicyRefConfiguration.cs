using HiveSpace.OrderService.Domain.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.External;

public class PlatformCurrencyPolicyRefConfiguration : IEntityTypeConfiguration<PlatformCurrencyPolicyRef>
{
    public void Configure(EntityTypeBuilder<PlatformCurrencyPolicyRef> builder)
    {
        builder.ToTable("platform_currency_policy_refs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DefaultCurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.EnabledCurrencyCodes).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
