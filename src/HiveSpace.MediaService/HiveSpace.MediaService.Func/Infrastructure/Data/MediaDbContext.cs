using HiveSpace.MediaService.Func.Core.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.MediaService.Func.Infrastructure.Data;

public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<MediaAsset> MediaAssets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }
}
