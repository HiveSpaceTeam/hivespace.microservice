using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ImportDuplicateGroupConfiguration : IEntityTypeConfiguration<ImportDuplicateGroup>
{
    public void Configure(EntityTypeBuilder<ImportDuplicateGroup> builder)
    {
        builder.ToTable("catalog_import_duplicate_groups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DuplicateKey).IsRequired().HasMaxLength(256);
        builder.Property(x => x.ExternalProductIdsJson).IsRequired();
        builder.Property(x => x.MemberImportedProductIdsJson).IsRequired();
        builder.Property(x => x.ResolutionStatus).IsRequired();
        builder.HasIndex(x => new { x.BundleId, x.DuplicateKey }).IsUnique();
    }
}
