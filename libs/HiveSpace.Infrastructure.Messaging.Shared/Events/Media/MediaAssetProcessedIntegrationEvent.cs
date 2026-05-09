using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Media;

public record MediaAssetProcessedIntegrationEvent(
    Guid FileId,
    string EntityType,
    string? EntityId,
    string PublicUrl,
    string? ThumbnailUrl) : IntegrationEvent;
