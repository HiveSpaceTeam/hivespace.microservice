using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ImportedSkuConfiguration : IEntityTypeConfiguration<ImportedSku>
{
    public void Configure(EntityTypeBuilder<ImportedSku> builder)
    {
        builder.ToTable("catalog_import_skus");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalSkuId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.SkuNumber).HasMaxLength(128);
        builder.Property(x => x.VariantSelectionsJson).IsRequired();
        builder.Property(x => x.SourceRawPrice).HasMaxLength(128);
        builder.Property(x => x.CurrencyCode).HasMaxLength(8);
        builder.Property(x => x.ImageUrlsJson);
        builder.Property(x => x.ReadinessStatus).IsRequired();
        builder.HasIndex(x => new { x.ImportedProductId, x.ExternalSkuId });
    }
}
