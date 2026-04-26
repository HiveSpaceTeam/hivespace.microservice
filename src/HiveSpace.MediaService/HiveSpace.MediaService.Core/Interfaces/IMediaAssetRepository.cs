using HiveSpace.MediaService.Core.DomainModels;

namespace HiveSpace.MediaService.Core.Interfaces;

public interface IMediaAssetRepository
{
    Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(MediaAsset mediaAsset);
    Task<List<MediaAsset>> GetExpiredPendingAsync(DateTimeOffset cutoffTime, int batchSize, CancellationToken ct = default);
    void RemoveRange(IEnumerable<MediaAsset> assets);
    Task SaveChangesAsync(CancellationToken ct = default);
}
