using HiveSpace.CatalogService.Application.Models.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.Data.Configurations
{
    public class StoreSnapshotValueConfiguration : IEntityTypeConfiguration<StoreSnapshot>
    {
        public void Configure(EntityTypeBuilder<StoreSnapshot> entity)
        {
            entity.ToTable("StoreSnapshots");
            entity.HasKey(av => av.Id);
        }
    }
}