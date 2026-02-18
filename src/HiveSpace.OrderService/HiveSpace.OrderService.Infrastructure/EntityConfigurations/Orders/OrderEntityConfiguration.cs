using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Orders;

public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.ShortId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.ShortId)
            .IsUnique();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // OwnsOne DeliveryAddress
        builder.OwnsOne(o => o.DeliveryAddress, address =>
        {
            address.Property(a => a.RecipientName).HasMaxLength(100).IsRequired();
            address.Property(a => a.StreetAddress).HasMaxLength(255).IsRequired();
            address.Property(a => a.Ward).HasMaxLength(100).IsRequired();
            address.Property(a => a.Province).HasMaxLength(100).IsRequired();
            address.Property(a => a.Country).HasMaxLength(100).IsRequired().HasDefaultValue("Vietnam");
            address.Property(a => a.Notes).HasMaxLength(500);

            // OwnsOne PhoneNumber inside DeliveryAddress
            address.OwnsOne(a => a.Phone, phone =>
            {
                phone.Property(p => p.Value)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20)
                    .IsRequired();
            });
        });

        // OwnsOne TotalAmount
        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount").IsRequired();
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3).IsRequired();
        });

        // HasMany Packages
        builder.HasMany(o => o.Packages)
            .WithOne() // OrderPackage doesn't seem to have OrderId property explicitly defined in domain, so usage shadow property or inferred
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // HasMany Trackings - configured in separate file
        builder.HasMany(o => o.Trackings)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Packages).AutoInclude(false);

        builder.ToTable("orders");
    }
}
