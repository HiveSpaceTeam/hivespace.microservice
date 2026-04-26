using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Infrastructure.Data;
using HiveSpace.MediaService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.MediaService.Core.Persistence.Repositories;

public class MediaAssetRepository(MediaDbContext dbContext) : IMediaAssetRepository
{
    public async Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.MediaAssets.FindAsync([id], ct);

    public void Add(MediaAsset mediaAsset)
        => dbContext.MediaAssets.Add(mediaAsset);

    public async Task<List<MediaAsset>> GetExpiredPendingAsync(DateTimeOffset cutoffTime, int batchSize, CancellationToken ct = default)
        => await dbContext.MediaAssets
            .Where(x => x.Status == MediaStatus.Pending && x.CreatedAt < cutoffTime)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public void RemoveRange(IEnumerable<MediaAsset> assets)
        => dbContext.MediaAssets.RemoveRange(assets);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await dbContext.SaveChangesAsync(ct);
}
