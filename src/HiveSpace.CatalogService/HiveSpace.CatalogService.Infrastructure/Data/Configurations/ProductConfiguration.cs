using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(255);
            builder.HasIndex(p => p.Slug).IsUnique();

            builder.Property(p => p.Description)
                .IsRequired();

            builder.Property(p => p.ShortDescription)
                .HasMaxLength(500);

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.Featured)
                .IsRequired();

            builder.Property(p => p.Condition)
                .IsRequired();

            builder.Property(p => p.SellerId)
                .IsRequired();

            builder.Property(p => p.BrandId);

            // Value Objects - Weight
            builder.OwnsOne(p => p.Weight, w =>
            {
                w.Property(p => p.Value).HasColumnName("WeightValue");
                w.Property(p => p.Unit).HasColumnName("WeightUnit");
            });

            // Value Objects - Dimensions
            builder.OwnsOne(p => p.Dimensions, d =>
            {
                d.Property(p => p.Length).HasColumnName("DimensionsLength");
                d.Property(p => p.Width).HasColumnName("DimensionsWidth");
                d.Property(p => p.Height).HasColumnName("DimensionsHeight");
                d.Property(p => p.Unit).HasColumnName("DimensionsUnit");
            });

            // Configure owned types (ValueObjects)
            builder.OwnsMany(p => p.Categories, pc =>
            {
                pc.HasKey(x => new { x.CategoryId, x.ProductId });
                pc.ToTable("ProductCategories");
                pc.WithOwner().HasForeignKey(x => x.ProductId);
            });

            builder.OwnsMany(p => p.Images, pi =>
            {
                pi.ToTable("ProductImages");
                pi.WithOwner().HasForeignKey(x => x.ProductId);
            });
        }
    }
}
