using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.BackgroundJobs;

public record BackgroundJobCompletedIntegrationEvent(
    Guid JobId,
    string OwningService,
    string OperationType,
    string Status,
    DateTimeOffset CompletedAt,
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
    string? OperatorSummary,
    string? CorrelationId) : IntegrationEvent;
