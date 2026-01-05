using HiveSpace.CatalogService.Domain.Aggregates.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class StoreRefValueConfiguration : IEntityTypeConfiguration<StoreRef>
    {
        public void Configure(EntityTypeBuilder<StoreRef> entity)
        {
            entity.ToTable("StoreRefs");
            entity.HasKey(av => av.Id);
        }
    }
}