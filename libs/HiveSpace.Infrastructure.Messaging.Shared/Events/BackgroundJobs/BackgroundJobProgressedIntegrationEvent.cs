using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.BackgroundJobs;

public record BackgroundJobProgressedIntegrationEvent(
    Guid JobId,
    string OwningService,
    string OperationType,
    string Status,
    DateTimeOffset ProgressedAt,
    int TotalCount,
    int ProcessedCount,
    int CreatedCount,
    int MatchedCount,
    int SkippedCount,
    int BlockedCount,
    int WarningCount,
    int DuplicateCount,
    int FailedCount,
    int ConflictCount,
    string? CorrelationId) : IntegrationEvent;
