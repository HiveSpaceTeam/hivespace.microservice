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
        }
    }
}
