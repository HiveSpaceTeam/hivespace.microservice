using HiveSpace.OrderService.Domain.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.OrderService.Infrastructure.EntityConfigurations.External;

public class SkuRefConfiguration : IEntityTypeConfiguration<SkuRef>
{
    public void Configure(EntityTypeBuilder<SkuRef> builder)
    {
        builder.ToTable("sku_refs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("bigint").ValueGeneratedNever();

        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.SkuNo).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SkuName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Price).HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.Attributes).HasColumnType("nvarchar(max)");
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.ProductId);
    }
}
