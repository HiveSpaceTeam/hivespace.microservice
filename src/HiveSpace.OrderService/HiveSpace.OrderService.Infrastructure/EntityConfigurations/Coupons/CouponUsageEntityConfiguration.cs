using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Coupons;

public class CouponUsageEntityConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("coupon_usages");
        
        builder.HasKey(u => u.Id);

        // Foreign Key
        builder.HasOne<Coupon>()
            .WithMany(c => c.Usages)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(u => u.UserId).IsRequired();
        builder.Property(u => u.OrderId).IsRequired();
        
        builder.OwnsOne(u => u.DiscountAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount");
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3);
        });
    }
}
