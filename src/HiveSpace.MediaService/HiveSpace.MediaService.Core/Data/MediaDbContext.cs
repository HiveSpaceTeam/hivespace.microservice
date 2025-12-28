using HiveSpace.Infrastructure.Persistence;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.MediaService.Core.Data;

public class MediaDbContext : DbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options) { }

    public DbSet<MediaAsset> MediaAssets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new MediaAssetEntityConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
