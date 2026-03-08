using HiveSpace.OrderService.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Orders;

public class OrderTrackingEntityConfiguration : IEntityTypeConfiguration<OrderTracking>
{
    public void Configure(EntityTypeBuilder<OrderTracking> builder)
    {
        builder.ToTable("order_trackings");
        
        builder.HasKey(t => t.Id);

        // Foreign Key
        builder.HasOne<Order>()
            .WithMany(o => o.Trackings)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(t => t.Type).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ExecutorType).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.Message).HasMaxLength(500);
    }
}
