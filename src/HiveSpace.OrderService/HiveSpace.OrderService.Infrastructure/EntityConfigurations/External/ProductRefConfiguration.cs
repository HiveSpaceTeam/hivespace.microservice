using HiveSpace.OrderService.Domain.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.External;

public class ProductRefConfiguration : IEntityTypeConfiguration<ProductRef>
{
    public void Configure(EntityTypeBuilder<ProductRef> builder)
    {
        builder.ToTable("product_refs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("bigint").ValueGeneratedNever();

        builder.Property(x => x.StoreId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.StoreId);
    }
}
