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
        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(c => c.Scope).HasConversion<string>().HasMaxLength(50).IsRequired();
        
        builder.Property(c => c.StartDateTime).IsRequired();
        builder.Property(c => c.EndDateTime).IsRequired();
        builder.Property(c => c.EarlySaveDateTime);
        builder.Property(c => c.IsHidden).HasDefaultValue(false);

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

        var longListComparer = new ValueComparer<IReadOnlyCollection<long>>(
            (c1, c2) => (c1 ?? new List<long>()).SequenceEqual(c2 ?? new List<long>()),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => (IReadOnlyCollection<long>)c.ToList());

        builder.Property(c => c.ApplicableProductIds)
            .HasColumnName("ApplicableProductIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<long>>(v, (JsonSerializerOptions?)null) ?? new List<long>(),
                longListComparer);

        builder.Property(c => c.StoreId);

        var intListComparer = new ValueComparer<IReadOnlyCollection<int>>(
            (c1, c2) => (c1 ?? new List<int>()).SequenceEqual(c2 ?? new List<int>()),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v)),
            c => (IReadOnlyCollection<int>)c.ToList());

        builder.Property(c => c.ApplicableCategoryIds)
            .HasColumnName("ApplicableCategoryIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>(),
                intListComparer);

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
