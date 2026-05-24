using HiveSpace.Infrastructure.Messaging.Shared.Events.Media;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Interfaces.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.MediaService.Core.Infrastructure.Messaging.Publishers;

public class MediaEventPublisher(IServiceProvider serviceProvider) : IMediaEventPublisher
{
    public Task PublishMediaAssetProcessedAsync(
        MediaAsset mediaAsset,
        string publicUrl,
        string? thumbnailUrl,
        CancellationToken cancellationToken = default)
        => serviceProvider.GetRequiredService<IPublishEndpoint>().Publish(new MediaAssetProcessedIntegrationEvent(
            mediaAsset.Id.ToString(),
            mediaAsset.EntityType,
            mediaAsset.EntityId,
            publicUrl,
            thumbnailUrl), cancellationToken);
}
