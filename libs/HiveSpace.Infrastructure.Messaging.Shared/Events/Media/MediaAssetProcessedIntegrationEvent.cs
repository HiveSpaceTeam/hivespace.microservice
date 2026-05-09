using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Media;

public record MediaAssetProcessedIntegrationEvent(
    string FileId,
    string EntityType,
    string? EntityId,
    string PublicUrl,
    string? ThumbnailUrl) : IntegrationEvent;
