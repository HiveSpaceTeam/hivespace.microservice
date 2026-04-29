using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class SkuConfiguration : IEntityTypeConfiguration<Sku>
{
    public void Configure(EntityTypeBuilder<Sku> entity)
    {
        entity.ToTable("skus");
        entity.HasKey(s => s.Id);

        entity.OwnsOne(s => s.Price, p =>
        {
            p.Property(m => m.Amount).HasColumnType("bigint");
            p.Property(m => m.Currency);
        });

        entity.OwnsMany(s => s.Images, si =>
        {
            si.ToTable("sku_images");
            si.WithOwner().HasForeignKey("SkuId");
        });

        entity.OwnsMany(s => s.SkuVariants, v =>
        {
            v.ToTable("sku_variants");
            v.WithOwner().HasForeignKey("SkuId");
            v.HasKey("Id");
        });
    }
}
