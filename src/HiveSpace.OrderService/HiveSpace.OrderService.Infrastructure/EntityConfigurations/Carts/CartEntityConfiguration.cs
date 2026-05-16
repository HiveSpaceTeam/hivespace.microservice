using HiveSpace.OrderService.Domain.Aggregates.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Carts;

public class CartEntityConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("CartId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.AppliedPlatformCoupons, coupon =>
        {
            coupon.ToTable("cart_platform_coupons");
            coupon.WithOwner().HasForeignKey("CartId");
            coupon.Property<int>("Id");
            coupon.HasKey("Id");
            coupon.Property(x => x.CouponCode).HasMaxLength(50).IsRequired();
            coupon.HasIndex("CartId", nameof(CartAppliedPlatformCoupon.CouponCode)).IsUnique();
        });

        builder.OwnsMany(x => x.AppliedStoreCoupons, coupon =>
        {
            coupon.ToTable("cart_store_coupons");
            coupon.WithOwner().HasForeignKey("CartId");
            coupon.Property<int>("Id");
            coupon.HasKey("Id");
            coupon.Property(x => x.StoreId).IsRequired();
            coupon.Property(x => x.CouponCode).HasMaxLength(50).IsRequired();
            coupon.HasIndex("CartId", nameof(CartAppliedStoreCoupon.StoreId)).IsUnique();
        });

        builder.Navigation(x => x.AppliedPlatformCoupons).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.AppliedStoreCoupons).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
