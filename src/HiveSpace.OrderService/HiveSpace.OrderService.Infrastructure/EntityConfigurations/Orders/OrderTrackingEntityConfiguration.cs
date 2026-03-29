using HiveSpace.Domain.Shared.Entities;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Orders;

public class OrderTrackingEntityConfiguration : IEntityTypeConfiguration<OrderTracking>
{
    public void Configure(EntityTypeBuilder<OrderTracking> builder)
    {
        builder.ToTable("order_trackings");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Type).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ExecutorType)
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<ExecutorType>(v))
            .HasMaxLength(50);
        builder.Property(t => t.Message).HasMaxLength(500);
    }
}
