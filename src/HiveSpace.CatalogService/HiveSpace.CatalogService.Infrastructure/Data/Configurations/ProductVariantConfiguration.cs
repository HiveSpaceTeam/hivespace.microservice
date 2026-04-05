using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> entity)
        {
            entity.ToTable("product_variants");
            entity.HasKey(pv => pv.Id);

            // Configure owned type collection (ValueObjects)
            entity.OwnsMany(pv => pv.Options, pvo =>
            {
                pvo.ToTable("product_variant_options");
                pvo.WithOwner().HasForeignKey("ProductVariantId");
            });
        }
    }
}
