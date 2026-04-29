using HiveSpace.CatalogService.Domain.Aggregates.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class StoreRefValueConfiguration : IEntityTypeConfiguration<StoreRef>
{
    public void Configure(EntityTypeBuilder<StoreRef> entity)
    {
        entity.ToTable("store_refs");
        entity.HasKey(av => av.Id);
    }
}
