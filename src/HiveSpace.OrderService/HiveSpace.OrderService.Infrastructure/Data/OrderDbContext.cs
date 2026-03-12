using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.EntityConfigurations.Carts;
using HiveSpace.OrderService.Infrastructure.EntityConfigurations.Coupons;
using HiveSpace.OrderService.Infrastructure.EntityConfigurations.External;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    // public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Coupon> Coupons { get; set; } = null!;
    public DbSet<CouponUsage> CouponUsages { get; set; } = null!;
    public DbSet<CouponRule> CouponRules { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<StoreRef> StoreRefs { get; set; } = null!;
    public DbSet<ProductRef> ProductRefs { get; set; } = null!;
    public DbSet<SkuRef> SkuRefs { get; set; } = null!;

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new CouponEntityConfiguration());
        builder.ApplyConfiguration(new CouponRuleEntityConfiguration());
        builder.ApplyConfiguration(new CouponUsageEntityConfiguration());
        builder.ApplyConfiguration(new CartEntityConfiguration());
        builder.ApplyConfiguration(new CartItemEntityConfiguration());
        builder.ApplyConfiguration(new StoreRefConfiguration());
        builder.ApplyConfiguration(new ProductRefConfiguration());
        builder.ApplyConfiguration(new SkuRefConfiguration());

        MassTransitExtensions.AddEntityOutBox(builder);
    }
}
