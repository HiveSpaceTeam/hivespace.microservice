using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HiveSpace.CatalogService.Infrastructure.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<AttributeDefinition> Attributes { get; set; }
        public DbSet<Sku> Skus { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
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
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);

                // Configure owned type collection (ValueObjects)
                entity.OwnsMany(c => c.CategoryAttributes, ca =>
                {
                    ca.HasKey(c => new { c.AttributeId, c.CategoryId });
                    ca.ToTable("CategoryAttributes");
                    ca.WithOwner().HasForeignKey(x => x.CategoryId);

                });
            });

            // Configure AttributeDefinition entity
            modelBuilder.Entity<AttributeDefinition>(entity =>
            {
                entity.ToTable("Attributes");
                entity.HasKey(a => a.Id);

                // Configure owned type (ValueObject) with explicit column names
                entity.OwnsOne(a => a.Type, at =>
                {
                    at.Property(t => t.ValueType).HasColumnName("ValueType");
                    at.Property(t => t.InputType).HasColumnName("InputType");
                    at.Property(t => t.IsMandatory).HasColumnName("IsMandatory");
                    at.Property(t => t.MaxValueCount).HasColumnName("MaxValueCount");
                });
            });

            // Configure Sku entity (as standalone entity)
            modelBuilder.Entity<Sku>(entity =>
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
            });

            // Configure ProductAttribute entity (as standalone entity)
            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.ToTable("ProductAttributes");
                entity.HasKey(pa => pa.Id);
                entity.Property(pa => pa.ProductId);
                entity.Property(pa => pa.AttributeId);
                entity.Property<string>(nameof(ProductAttribute.FreeTextValue));

                var comparer = new ValueComparer<List<Guid>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? new List<Guid>() : c.ToList());

                entity.Property<List<Guid>>("_selectedValueIds")
                    .HasColumnName("SelectedValueIds")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v ?? new List<Guid>(), (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<Guid>>(v ?? "[]", (JsonSerializerOptions?)null) ?? new List<Guid>())
                    .Metadata.SetValueComparer(comparer);

                entity.HasOne<Product>()
                    .WithMany(p => p.Attributes)
                    .HasForeignKey(pa => pa.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ProductVariant entity (as standalone entity)
            modelBuilder.Entity<ProductVariant>(entity =>
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
            });

            // Configure AttributeValue entity
            modelBuilder.Entity<AttributeValue>(entity =>
            {
                entity.ToTable("AttributeValues");
                entity.HasKey(av => av.Id);
            });
        }
    }
}
