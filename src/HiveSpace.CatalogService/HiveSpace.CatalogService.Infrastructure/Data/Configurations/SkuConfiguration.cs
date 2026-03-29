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
                si.ToTable("SkuImages");
                si.WithOwner().HasForeignKey("SkuId");
            });

            entity.OwnsMany(s => s.SkuVariants, v =>
            {
                v.ToTable("SkuVariants");
                v.WithOwner().HasForeignKey("SkuId");
                v.HasKey("Id");
            });
        }
    }
}
