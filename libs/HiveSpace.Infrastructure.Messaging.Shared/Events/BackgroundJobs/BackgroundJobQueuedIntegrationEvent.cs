using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.BackgroundJobs;

public record BackgroundJobQueuedIntegrationEvent(
    Guid JobId,
    string OwningService,
    string OperationType,
    string Status,
    DateTimeOffset QueuedAt,
    string? SourceFingerprint,
    Guid? BundleId,
    Guid RequestedByUserId,
    string? CorrelationId) : IntegrationEvent;
