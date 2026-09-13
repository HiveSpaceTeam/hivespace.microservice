using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ImportedAttributeConfiguration : IEntityTypeConfiguration<ImportedAttribute>
{
    public void Configure(EntityTypeBuilder<ImportedAttribute> builder)
    {
        builder.ToTable("catalog_import_attributes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalAttributeName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.ExternalAttributeValue);
        builder.Property(x => x.SourceAttributeId).HasMaxLength(128);
        builder.Property(x => x.MatchStatus).IsRequired();
    }
}
