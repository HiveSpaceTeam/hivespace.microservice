using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class SellerOwnershipLinkConfiguration : IEntityTypeConfiguration<SellerOwnershipLink>
{
    public void Configure(EntityTypeBuilder<SellerOwnershipLink> builder)
    {
        builder.ToTable("catalog_import_seller_ownership_links");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceSystem).IsRequired().HasMaxLength(64);
        builder.Property(x => x.ExternalSellerId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.LinkStatus).IsRequired();
        builder.Property(x => x.ApprovalReason).HasMaxLength(1000);

        builder.HasIndex(x => new { x.SourceSystem, x.ExternalSellerId, x.LinkStatus });
    }
}
