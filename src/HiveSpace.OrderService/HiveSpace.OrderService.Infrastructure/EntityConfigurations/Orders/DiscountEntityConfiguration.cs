using HiveSpace.OrderService.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Orders;

public class DiscountEntityConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("order_discounts");
        
        builder.HasKey(d => d.Id);

        // Foreign Key
        builder.HasOne<OrderPackage>()
            .WithMany(p => p.Discounts)
            .HasForeignKey("OrderPackageId") // Shadow property
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(d => d.CouponCode).HasMaxLength(50);
        builder.Property(d => d.Scope).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(d => d.DiscountAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount");
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3);
        });
    }
}
