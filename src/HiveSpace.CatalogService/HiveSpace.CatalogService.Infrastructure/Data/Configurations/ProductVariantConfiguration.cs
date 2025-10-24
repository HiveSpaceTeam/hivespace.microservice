using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> entity)
        {
            entity.ToTable("ProductVariants");
            entity.HasKey(pv => pv.Id);
            entity.HasOne<Product>()
                .WithMany(p => p.Variants)
                .HasForeignKey("ProductId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure owned type collection (ValueObjects)
            entity.OwnsMany(pv => pv.Options, pvo =>
            {
                pvo.ToTable("ProductVariantOptions");
                pvo.WithOwner().HasForeignKey(x => x.ProductVariantId);
                pvo.HasKey(x => new { x.OptionId, x.ProductVariantId });
            });
        }
    }
}
