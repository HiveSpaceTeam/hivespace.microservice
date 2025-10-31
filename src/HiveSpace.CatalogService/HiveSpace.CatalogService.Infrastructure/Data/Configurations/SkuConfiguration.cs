using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class SkuConfiguration : IEntityTypeConfiguration<Sku>
    {
        public void Configure(EntityTypeBuilder<Sku> entity)
        {
            entity.ToTable("Skus");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.ProductId);
            entity.HasOne<Product>()
                .WithMany(p => p.Skus)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure owned type (ValueObject)
            entity.OwnsOne(s => s.Price, p =>
            {
                p.Property(m => m.Amount)
                    .HasColumnType("decimal(18,2)");
                p.Property(m => m.Currency);
            });

            // Configure owned type collection (ValueObjects)
            entity.OwnsMany(s => s.Images, si =>
            {
                si.HasKey(x => new { x.FileId, x.SkuId });
                si.ToTable("SkuImages");
                si.WithOwner().HasForeignKey(x => x.SkuId);
            });

            entity.OwnsMany(s => s.SkuVariants, sv =>
            {
                sv.ToTable("SkuVariants");
                sv.WithOwner().HasForeignKey(x => x.SkuId);
                sv.HasKey(x => new { x.SkuId, x.VariantId, x.OptionId });
                // Establish FK from SkuVariant.VariantId to ProductVariant.Id
                sv.HasOne<ProductVariant>()
                    .WithMany()
                    .HasForeignKey(x => x.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
