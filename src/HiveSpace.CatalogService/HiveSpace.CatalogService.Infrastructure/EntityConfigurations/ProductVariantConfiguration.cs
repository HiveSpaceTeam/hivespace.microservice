using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> entity)
    {
        entity.ToTable("product_variants");
        entity.HasKey(pv => pv.Id);

        entity.OwnsMany(pv => pv.Options, pvo =>
        {
            pvo.ToTable("product_variant_options");
            pvo.WithOwner().HasForeignKey("ProductVariantId");
        });
    }
}
