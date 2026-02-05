using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using Microsoft.EntityFrameworkCore;


using HiveSpace.OrderService.Domain.Aggregates.Carts;

namespace HiveSpace.OrderService.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Coupon> Coupons { get; set; } = null!;
    public DbSet<CouponUsage> CouponUsages { get; set; } = null!;
    public DbSet<CouponRule> CouponRules { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;


    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Only apply configurations from Infrastructure assembly (not Domain)
        builder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        // Add MassTransit outbox entities
        MassTransitExtensions.AddEntityOutBox(builder);
    }
}
