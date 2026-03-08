using System.Text.Json;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Coupons;

public class CouponEntityConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);
        builder.ToTable("coupons");

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.Name).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(c => c.Scope).HasConversion<string>().HasMaxLength(50).IsRequired();

        // Money VOs
        builder.OwnsOne(c => c.DiscountAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("DiscountAmount");
            money.Property(m => m.Currency).HasColumnName("DiscountCurrency").HasConversion<string>().HasMaxLength(3);
        });

        builder.OwnsOne(c => c.MaxDiscountAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("MaxDiscountAmount");
            money.Property(m => m.Currency).HasColumnName("MaxDiscountCurrency").HasConversion<string>().HasMaxLength(3);
        });

        builder.OwnsOne(c => c.MinOrderAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("MinOrderAmount");
            money.Property(m => m.Currency).HasColumnName("MinOrderCurrency").HasConversion<string>().HasMaxLength(3);
        });

        var guidListComparer = new ValueComparer<IReadOnlyCollection<Guid>>(
            (c1, c2) => (c1 ?? new List<Guid>()).SequenceEqual(c2 ?? new List<Guid>()),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => (IReadOnlyCollection<Guid>)c.ToList());

        builder.Property(c => c.ApplicableProductIds)
            .HasColumnName("ApplicableProductIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>(),
                guidListComparer);

        builder.Property(c => c.ApplicableStoreIds)
            .HasColumnName("ApplicableStoreIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>(),
                guidListComparer);

        // Rules Collection - configured in CouponRuleEntityConfiguration
        builder.HasMany(c => c.Rules)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        // Usages Collection - configured in CouponUsageEntityConfiguration
        builder.HasMany(c => c.Usages)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
