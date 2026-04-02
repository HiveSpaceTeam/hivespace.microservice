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
            entity.Property(c => c.Id).ValueGeneratedNever();
            entity.Property(c => c.IsActive).HasColumnName("IsActive");
            entity.Property(c => c.FilePath).HasColumnName("FilePath").HasMaxLength(500);

            // Configure owned type collection (ValueObjects)
            entity.OwnsMany(c => c.CategoryAttributes, ca =>
            {
                ca.Property(x => x.AttributeId).ValueGeneratedNever();
                ca.Property(x => x.CategoryId).ValueGeneratedNever();
                ca.HasKey(c => new { c.AttributeId, c.CategoryId });
                ca.ToTable("CategoryAttributes");
                ca.WithOwner().HasForeignKey(x => x.CategoryId);
            });
        }
    }
}
