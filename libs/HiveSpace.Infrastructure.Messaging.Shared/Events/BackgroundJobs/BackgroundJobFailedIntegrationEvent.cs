using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.BackgroundJobs;

public record BackgroundJobFailedIntegrationEvent(
    Guid JobId,
    string OwningService,
    string OperationType,
    string Status,
    DateTimeOffset FailedAt,
    string ErrorSummary,
    string? CorrelationId) : IntegrationEvent;
