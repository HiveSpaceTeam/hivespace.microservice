using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ImportedSellerConfiguration : IEntityTypeConfiguration<ImportedSeller>
{
    public void Configure(EntityTypeBuilder<ImportedSeller> builder)
    {
        builder.ToTable("catalog_import_sellers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalSellerId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ExternalSellerSlug).HasMaxLength(256);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.SourceUrl).HasMaxLength(1000);
        builder.Property(x => x.LogoUrl).IsRequired().HasMaxLength(500);
        builder.Property(x => x.MetadataJson);
        builder.Property(x => x.ProvisioningStatus).IsRequired();
        builder.Property(x => x.ConflictReason).HasMaxLength(512);
        builder.Property(x => x.SuggestedHiveSpaceStoreId);
        builder.Property(x => x.SuggestedHiveSpaceUserId);
        builder.HasIndex(x => new { x.BundleId, x.ExternalSellerId }).IsUnique();
    }
}
