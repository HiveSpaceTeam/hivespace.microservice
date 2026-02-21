using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.Domain.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Orders;

public class OrderPackageEntityConfiguration : IEntityTypeConfiguration<OrderPackage>
{
    public void Configure(EntityTypeBuilder<OrderPackage> builder)
    {
        builder.HasKey(p => p.Id);

        builder.ToTable("order_packages");

        builder.Property(p => p.StoreId).IsRequired();
        builder.Property(p => p.BuyerId).IsRequired();
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(p => p.RejectionReason).HasMaxLength(500);

        // Map Money VOs with custom column prefixes
        builder.OwnsOne(p => p.SubTotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("SubTotalAmount");
            money.Property(m => m.Currency).HasColumnName("SubTotalCurrency").HasConversion<string>().HasMaxLength(3);
        });

        builder.OwnsOne(p => p.TotalDiscount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalDiscountAmount");
            money.Property(m => m.Currency).HasColumnName("TotalDiscountCurrency").HasConversion<string>().HasMaxLength(3);
        });

        builder.OwnsOne(p => p.ShippingFee, money =>
        {
            money.Property(m => m.Amount).HasColumnName("ShippingFeeAmount");
            money.Property(m => m.Currency).HasColumnName("ShippingFeeCurrency").HasConversion<string>().HasMaxLength(3);
        });

        builder.OwnsOne(p => p.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount");
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3);
        });

        // Collections
        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey("OrderPackageId")
            .OnDelete(DeleteBehavior.Cascade);

        // HasMany Checkouts - configured in CheckoutEntityConfiguration
        builder.HasMany(p => p.Checkouts)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        // HasMany Discounts - configured in DiscountEntityConfiguration
        builder.HasMany(p => p.Discounts)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
