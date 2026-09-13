using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class CatalogImportBundleConfiguration : IEntityTypeConfiguration<CatalogImportBundle>
{
    public void Configure(EntityTypeBuilder<CatalogImportBundle> builder)
    {
        builder.ToTable("catalog_import_bundles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SchemaVersion).IsRequired().HasMaxLength(32);
        builder.Property(x => x.SourceSystem).IsRequired().HasMaxLength(64);
        builder.Property(x => x.SourceType).IsRequired().HasMaxLength(32);
        builder.Property(x => x.SourceValue).IsRequired().HasMaxLength(512);
        builder.Property(x => x.SourceUrl).HasMaxLength(1000);
        builder.Property(x => x.SourceFingerprint).IsRequired().HasMaxLength(128);
        builder.Property(x => x.SourceFileName).HasMaxLength(512);
        builder.HasIndex(x => x.SourceFingerprint).IsUnique();
        builder.Property(x => x.CheckpointId).HasMaxLength(128);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.SubmittedByUserId).IsRequired();
        builder.Property(x => x.CrawledAt).IsRequired();
        builder.Property(x => x.SubmittedAt).IsRequired();
        builder.Ignore(x => x.CategoryLinks);

        builder.Navigation(x => x.Sellers).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.CategoryMappings).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Products).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.ValidationIssues).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.DuplicateGroups).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.SellerOwnershipLinks).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Sellers)
            .WithOne()
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CategoryMappings)
            .WithOne()
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Products)
            .WithOne()
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ValidationIssues)
            .WithOne()
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.DuplicateGroups)
            .WithOne()
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SellerOwnershipLinks)
            .WithOne()
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
