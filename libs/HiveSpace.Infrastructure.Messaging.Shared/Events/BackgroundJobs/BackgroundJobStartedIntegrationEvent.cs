using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.BackgroundJobs;

public record BackgroundJobStartedIntegrationEvent(
    Guid JobId,
    string OwningService,
    string OperationType,
    string Status,
    DateTimeOffset StartedAt,
    string? SourceFingerprint,
    Guid? BundleId,
    string? CorrelationId) : IntegrationEvent;
