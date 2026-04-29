using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");
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
                w.Property(p => p.Value).HasColumnName("WeightValue").HasColumnType("decimal(18,2)");
                w.Property(p => p.Unit).HasColumnName("WeightUnit");
            });

            // Value Objects - Dimensions
            builder.OwnsOne(p => p.Dimensions, d =>
            {
                d.Property(p => p.Length).HasColumnName("DimensionsLength").HasColumnType("decimal(18,2)");
                d.Property(p => p.Width).HasColumnName("DimensionsWidth").HasColumnType("decimal(18,2)");
                d.Property(p => p.Height).HasColumnName("DimensionsHeight").HasColumnType("decimal(18,2)");
                d.Property(p => p.Unit).HasColumnName("DimensionsUnit");
            });
            builder.OwnsMany(p => p.Categories, pc =>
            {
                pc.ToTable("product_categories");

                pc.WithOwner().HasForeignKey("ProductId"); // shadow FK

                pc.Property<int>("Id");
                pc.HasKey("Id");

                pc.Property(x => x.CategoryId).IsRequired();
            });

            builder.OwnsMany(p => p.Attributes, pa =>
            {
                pa.ToTable("product_attributes");

                pa.WithOwner().HasForeignKey("ProductId"); // shadow FK

                pa.Property<int>("Id");
                pa.HasKey("Id");

                pa.Property(x => x.AttributeId).IsRequired();

                pa.Property<string>(nameof(ProductAttribute.FreeTextValue));

                var comparer = new ValueComparer<List<int>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? new List<int>() : c.ToList());

                pa.Property<List<int>>("_selectedValueIds")
                    .HasColumnName("SelectedValueIds")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v ?? new List<int>(), (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<int>>(v ?? "[]", (JsonSerializerOptions?)null) ?? new List<int>())
                    .Metadata.SetValueComparer(comparer);
            });

            builder.OwnsMany(p => p.Images, pi =>
            {
                pi.ToTable("product_images");
                pi.WithOwner().HasForeignKey(x => x.ProductId);
            });

            builder.HasMany(p => p.Variants)
               .WithOne()
               .HasForeignKey("ProductId");

            builder.HasMany(p => p.Skus)
                   .WithOne()
                   .HasForeignKey("ProductId");
    }
}
