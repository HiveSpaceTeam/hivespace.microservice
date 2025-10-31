using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entity)
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
        }
    }
}
