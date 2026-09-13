using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ImportValidationIssueConfiguration : IEntityTypeConfiguration<ImportValidationIssue>
{
    public void Configure(EntityTypeBuilder<ImportValidationIssue> builder)
    {
        builder.ToTable("catalog_import_validation_issues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(64);
        builder.Property(x => x.EntitySourceId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Field).HasMaxLength(128);
        builder.Property(x => x.Severity).IsRequired();
        builder.Property(x => x.ReasonCode).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.MetadataJson);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => new { x.BundleId, x.EntityType, x.EntitySourceId });
    }
}
