using System.Text.Json;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.Domain.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Orders;

public class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.ToTable("order_items");

        builder.Property(i => i.ProductId).IsRequired(); // Guid
        builder.Property(i => i.SkuId).IsRequired(); // Guid

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPriceAmount");
            money.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasConversion<string>().HasMaxLength(3);
        });

        builder.OwnsOne(i => i.LineTotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("LineTotalAmount");
            money.Property(m => m.Currency).HasColumnName("LineTotalCurrency").HasConversion<string>().HasMaxLength(3);
        });

        builder.OwnsOne(i => i.ProductSnapshot, snapshot =>
        {
            snapshot.Property(s => s.ProductId).HasColumnName("SnapshotProductId"); // Long
            snapshot.Property(s => s.SkuId).HasColumnName("SnapshotSkuId"); // Long
            snapshot.Property(s => s.ProductName).HasColumnName("SnapshotProductName").HasMaxLength(255);
            snapshot.Property(s => s.SkuName).HasColumnName("SnapshotSkuName").HasMaxLength(255);
            snapshot.Property(s => s.ImageUrl).HasColumnName("SnapshotImageUrl").HasMaxLength(500);

            // Snapshot Price
            snapshot.OwnsOne(s => s.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("SnapshotPriceAmount");
                money.Property(m => m.Currency).HasColumnName("SnapshotPriceCurrency").HasConversion<string>().HasMaxLength(3);
            });

            // Attributes Dictionary to JSON
            snapshot.Property(s => s.Attributes)
                .HasColumnName("SnapshotAttributes")
                .HasConversion(
                    d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>(),
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyDictionary<string, string>>(
                        (c1, c2) => (c1 ?? new Dictionary<string, string>()).SequenceEqual(c2 ?? new Dictionary<string, string>()),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToDictionary(k => k.Key, v => v.Value)
                    )
                );
        });
    }
}
