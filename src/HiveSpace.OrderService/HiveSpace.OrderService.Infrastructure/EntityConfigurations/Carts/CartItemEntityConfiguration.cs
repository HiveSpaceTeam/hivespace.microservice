using HiveSpace.OrderService.Domain.Aggregates.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.Carts;

public class CartItemEntityConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ProductId).HasColumnType("bigint").IsRequired();
        builder.Property(x => x.SkuId).HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.IsSelected).IsRequired().HasDefaultValue(true).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.Property<Guid>("CartId").IsRequired();
        builder.HasIndex("CartId");
        builder.HasIndex(x => new { x.ProductId, x.SkuId });
    }
}
