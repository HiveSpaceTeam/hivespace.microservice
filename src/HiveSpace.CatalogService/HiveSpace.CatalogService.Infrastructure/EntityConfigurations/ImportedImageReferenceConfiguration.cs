using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ImportedImageReferenceConfiguration : IEntityTypeConfiguration<ImportedImageReference>
{
    public void Configure(EntityTypeBuilder<ImportedImageReference> builder)
    {
        builder.ToTable("catalog_import_image_references");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalUrl).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.SourceImageId).HasMaxLength(128);
        builder.Property(x => x.Role).IsRequired().HasMaxLength(64);
        builder.Property(x => x.MediaStatus).IsRequired();
        builder.Property(x => x.MediaFileId).HasMaxLength(100);
    }
}
