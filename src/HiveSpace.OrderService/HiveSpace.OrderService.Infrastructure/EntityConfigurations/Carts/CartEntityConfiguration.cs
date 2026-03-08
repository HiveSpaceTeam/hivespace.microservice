using HiveSpace.OrderService.Domain.Aggregates.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Carts;

public class CartEntityConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        var itemsComparer = new ValueComparer<IReadOnlyCollection<CartItem>>(
            (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
            c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
            c => JsonSerializer.Deserialize<List<CartItem>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new List<CartItem>());

        builder.Property(c => c.Items)
            .HasField("_items")
            .HasColumnName("Items")
            .HasColumnType("nvarchar(max)") // Explicitly string for SQL Server
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<CartItem>>(v, (JsonSerializerOptions?)null) ?? new List<CartItem>(),
                itemsComparer);
    }
}
