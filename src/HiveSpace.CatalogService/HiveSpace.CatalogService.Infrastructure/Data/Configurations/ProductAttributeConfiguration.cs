using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.Json;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductAttribute> entity)
        {
            entity.ToTable("ProductAttributes");
            entity.HasKey(pa => pa.Id);
            entity.Property(pa => pa.ProductId);
            entity.Property(pa => pa.AttributeId);
            entity.Property<string>(nameof(ProductAttribute.FreeTextValue));

            var comparer = new ValueComparer<List<int>>(
                (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<int>() : c.ToList());

            entity.Property<List<int>>("_selectedValueIds")
                .HasColumnName("SelectedValueIds")
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<int>(), (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<int>>(v ?? "[]", (JsonSerializerOptions?)null) ?? new List<int>())
                .Metadata.SetValueComparer(comparer);
    
            entity.HasOne<Product>()
                .WithMany(p => p.Attributes)
                .HasForeignKey(pa => pa.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
