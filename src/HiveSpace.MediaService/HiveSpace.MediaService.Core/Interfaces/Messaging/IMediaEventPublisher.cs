using HiveSpace.MediaService.Core.DomainModels;

namespace HiveSpace.MediaService.Core.Interfaces.Messaging;

public interface IMediaEventPublisher
{
    Task PublishMediaAssetProcessedAsync(
        MediaAsset mediaAsset,
        string publicUrl,
        string? thumbnailUrl,
        CancellationToken cancellationToken = default);
}
