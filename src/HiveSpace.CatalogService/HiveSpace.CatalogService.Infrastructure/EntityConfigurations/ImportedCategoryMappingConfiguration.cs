using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ImportedCategoryMappingConfiguration : IEntityTypeConfiguration<ImportedCategoryMapping>
{
    public void Configure(EntityTypeBuilder<ImportedCategoryMapping> builder)
    {
        builder.ToTable("catalog_import_category_mappings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ExternalCategoryId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ExternalParentCategoryId).HasMaxLength(128);
        builder.Property(x => x.ExternalCategoryName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PathJson);
        builder.Property(x => x.MappingStatus).IsRequired();
        builder.HasIndex(x => new { x.BundleId, x.ExternalCategoryId }).IsUnique();
    }
}
