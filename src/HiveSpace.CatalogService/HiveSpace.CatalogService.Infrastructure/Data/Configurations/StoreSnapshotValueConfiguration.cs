using HiveSpace.CatalogService.Domain.Aggregates.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class StoreSnapshotValueConfiguration : IEntityTypeConfiguration<StoreRef>
    {
        public void Configure(EntityTypeBuilder<StoreRef> entity)
        {
            entity.ToTable("StoreSnapshots");
            entity.HasKey(av => av.Id);
        }
    }
}