using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.ToTable("categories");
        entity.HasKey(c => c.Id);
        entity.Property(c => c.Id).ValueGeneratedNever();
        entity.Property(c => c.FilePath).HasMaxLength(500);

        entity.OwnsMany(c => c.CategoryAttributes, ca =>
        {
            ca.Property(x => x.AttributeId).ValueGeneratedNever();
            ca.Property(x => x.CategoryId).ValueGeneratedNever();
            ca.HasKey(c => new { c.AttributeId, c.CategoryId });
            ca.ToTable("category_attributes");
            ca.WithOwner().HasForeignKey(x => x.CategoryId);
        });
    }
}
