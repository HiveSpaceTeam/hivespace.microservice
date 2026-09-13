using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class ExternalCategoryLinkConfiguration : IEntityTypeConfiguration<ExternalCategoryLink>
{
    public void Configure(EntityTypeBuilder<ExternalCategoryLink> builder)
    {
        builder.ToTable("external_category_links");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SourceSystem).HasColumnName("source_system").IsRequired().HasMaxLength(64);
        builder.Property(x => x.ExternalCategoryId).HasColumnName("external_category_id").IsRequired().HasMaxLength(128);
        builder.Property(x => x.ExternalCategoryName).HasColumnName("external_category_name").IsRequired().HasMaxLength(256);
        builder.Property(x => x.ExternalParentCategoryId).HasColumnName("external_parent_category_id").HasMaxLength(128);
        builder.Property(x => x.PathJson).HasColumnName("path_json");
        builder.Property(x => x.HiveSpaceCategoryId).HasColumnName("hive_space_category_id").IsRequired();
        builder.Property(x => x.ProvisioningStatus).HasColumnName("provisioning_status").IsRequired();
        builder.Property(x => x.ConflictReason).HasColumnName("conflict_reason").HasMaxLength(512);
        builder.Property(x => x.SourceFingerprint).HasColumnName("source_fingerprint").IsRequired().HasMaxLength(128);
        builder.Property(x => x.ProvisionedByUserId).HasColumnName("provisioned_by_user_id").IsRequired();
        builder.Property(x => x.ProvisionedAt).HasColumnName("provisioned_at").IsRequired();
        builder.HasIndex(x => new { x.SourceSystem, x.ExternalCategoryId }).IsUnique();
    }
}
