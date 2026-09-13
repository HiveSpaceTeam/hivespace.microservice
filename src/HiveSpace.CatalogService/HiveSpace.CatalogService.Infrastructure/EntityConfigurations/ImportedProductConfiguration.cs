using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ImportedProductConfiguration : IEntityTypeConfiguration<ImportedProduct>
{
    public void Configure(EntityTypeBuilder<ImportedProduct> builder)
    {
        builder.ToTable("catalog_import_products");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalProductId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ExternalProductUrl).HasMaxLength(1000);
        builder.Property(x => x.ExternalSellerId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Description);
        builder.Property(x => x.ExternalCategoryIdsJson).IsRequired();
        builder.Property(x => x.ThumbnailImageExternalUrl).HasMaxLength(1000);
        builder.Property(x => x.ReadinessStatus).IsRequired();
        builder.Property(x => x.ImportStatus).IsRequired();
        builder.HasIndex(x => new { x.BundleId, x.ExternalProductId });

        builder.Navigation(x => x.Skus).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Attributes).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Images).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Skus)
            .WithOne()
            .HasForeignKey(x => x.ImportedProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attributes)
            .WithOne()
            .HasForeignKey(x => x.ImportedProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey(x => x.ImportedProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
