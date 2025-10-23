using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> entity)
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);

            // Configure owned types (ValueObjects)
            entity.OwnsMany(p => p.Categories, pc =>
            {
                pc.HasKey(x => new { x.CategoryId, x.ProductId });
                pc.ToTable("ProductCategories");
                pc.WithOwner().HasForeignKey(x => x.ProductId);
            });

            entity.OwnsMany(p => p.Images, pi =>
            {
                pi.ToTable("ProductImages");
                pi.WithOwner().HasForeignKey(x => x.ProductId);
            });
        }
    }
}
