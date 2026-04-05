using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Orders;

public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.ToTable("orders");

        builder.Property(o => o.ShortId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.ShortId)
            .IsUnique();

        builder.Property(o => o.StoreId).IsRequired();

        builder.Property(o => o.Status)
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<OrderStatus>(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.RejectionReason).HasMaxLength(500);

        // OwnsOne DeliveryAddress
        builder.OwnsOne(o => o.DeliveryAddress, address =>
        {
            address.Property(a => a.RecipientName).HasMaxLength(100).IsRequired();
            address.Property(a => a.StreetAddress).HasMaxLength(255).IsRequired();
            address.Property(a => a.Commune).HasMaxLength(100).IsRequired();
            address.Property(a => a.Province).HasMaxLength(100).IsRequired();
            address.Property(a => a.Country).HasMaxLength(100).IsRequired().HasDefaultValue("Vietnam");
            address.Property(a => a.Notes).HasMaxLength(500);

            address.OwnsOne(a => a.Phone, phone =>
            {
                phone.Property(p => p.Value)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20)
                    .IsRequired();
            });
        });

        // Financial Money VOs
        builder.OwnsOne(o => o.SubTotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("SubTotalAmount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("SubTotalCurrency").HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(o => o.TotalDiscount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalDiscountAmount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("TotalDiscountCurrency").HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(o => o.ShippingFee, money =>
        {
            money.Property(m => m.Amount).HasColumnName("ShippingFeeAmount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("ShippingFeeCurrency").HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        // OwnsMany Checkouts (value objects — shadow PK)
        builder.OwnsMany(o => o.Checkouts, cb =>
        {
            cb.ToTable("order_checkouts");
            cb.WithOwner().HasForeignKey("OrderId");
            cb.Property<Guid>("Id");
            cb.HasKey("Id");

            cb.Property(c => c.PaymentMethod)
                .HasConversion(
                    v => v.Name,
                    v => Enumeration.FromDisplayName<PaymentMethod>(v))
                .HasMaxLength(50)
                .IsRequired();

            cb.OwnsOne(c => c.Amount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Amount").IsRequired();
                money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3).IsRequired();
            });
        });

        // OwnsMany Discounts (value objects — shadow PK)
        builder.OwnsMany(o => o.Discounts, db =>
        {
            db.ToTable("order_discounts");
            db.WithOwner().HasForeignKey("OrderId");
            db.Property<Guid>("Id");
            db.HasKey("Id");

            db.Property(d => d.CouponCode).HasMaxLength(50).IsRequired();
            db.Property(d => d.CouponOwnerType).HasConversion<string>().HasMaxLength(50).IsRequired();
            db.Property(d => d.Scope).HasConversion<string>().HasMaxLength(50).IsRequired();

            db.OwnsOne(d => d.DiscountAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Amount").IsRequired();
                money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3).IsRequired();
            });
        });

        // HasMany Items
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // HasMany Trackings
        builder.HasMany(o => o.Trackings)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
