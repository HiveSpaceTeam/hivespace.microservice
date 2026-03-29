using HiveSpace.Domain.Shared.Entities;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Orders;

public class CheckoutEntityConfiguration : IEntityTypeConfiguration<Checkout>
{
    public void Configure(EntityTypeBuilder<Checkout> builder)
    {
        builder.ToTable("order_checkouts");

        builder.HasKey(c => c.Id);

        // Foreign Key
        builder.HasOne<OrderPackage>()
            .WithMany(p => p.Checkouts)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.PaymentMethod)
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<PaymentMethod>(v))
            .HasMaxLength(50);
            
        builder.OwnsOne(c => c.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount");
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3);
        });
    }
}
