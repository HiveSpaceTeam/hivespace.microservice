using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ExternalCategoryAttributeLinkConfiguration : IEntityTypeConfiguration<ExternalCategoryAttributeLink>
{
    public void Configure(EntityTypeBuilder<ExternalCategoryAttributeLink> builder)
    {
        builder.ToTable("external_category_attribute_links");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceSystem).IsRequired().HasMaxLength(64);
        builder.Property(x => x.ExternalCategoryId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.SourceAttributeId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ExternalAttributeName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.InputType).IsRequired().HasMaxLength(64);
        builder.Property(x => x.SelectableValuesJson).IsRequired();
        builder.Property(x => x.SourceFingerprint).IsRequired().HasMaxLength(256);
        builder.HasIndex(x => new { x.SourceSystem, x.ExternalCategoryId, x.SourceAttributeId }).IsUnique();
    }
}
