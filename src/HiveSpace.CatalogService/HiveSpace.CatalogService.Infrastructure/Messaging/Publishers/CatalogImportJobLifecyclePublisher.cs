using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.BackgroundJobs;

namespace HiveSpace.CatalogService.Infrastructure.Messaging.Publishers;

public class CatalogImportJobLifecyclePublisher(IEventPublisher eventPublisher) : ICatalogImportJobLifecyclePublisher
{
    private const string OwningService = "CatalogService";

    public Task PublishQueuedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => eventPublisher.PublishAsync(new BackgroundJobQueuedIntegrationEvent(
            job.Id,
            OwningService,
            job.OperationType.ToString(),
            job.Status.ToString(),
            job.RequestedAt,
            job.SourceFingerprint,
            job.BundleId,
            job.RequestedByUserId,
            job.CorrelationId), cancellationToken);

    public Task PublishStartedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => eventPublisher.PublishAsync(new BackgroundJobStartedIntegrationEvent(
            job.Id,
            OwningService,
            job.OperationType.ToString(),
            job.Status.ToString(),
            job.StartedAt ?? DateTimeOffset.UtcNow,
            job.SourceFingerprint,
            job.BundleId,
            job.CorrelationId), cancellationToken);

    public Task PublishProgressedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => eventPublisher.PublishAsync(new BackgroundJobProgressedIntegrationEvent(
            job.Id,
            OwningService,
            job.OperationType.ToString(),
            job.Status.ToString(),
            DateTimeOffset.UtcNow,
            job.TotalCount,
            job.ProcessedCount,
            job.CreatedCount,
            job.MatchedCount,
            job.SkippedCount,
            job.BlockedCount,
            job.WarningCount,
            job.DuplicateCount,
            job.FailedCount,
            job.ConflictCount,
            job.CorrelationId), cancellationToken);

    public Task PublishCompletedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => eventPublisher.PublishAsync(new BackgroundJobCompletedIntegrationEvent(
            job.Id,
            OwningService,
            job.OperationType.ToString(),
            job.Status.ToString(),
            job.CompletedAt ?? DateTimeOffset.UtcNow,
            job.TotalCount,
            job.ProcessedCount,
            job.CreatedCount,
            job.MatchedCount,
            job.SkippedCount,
            job.BlockedCount,
            job.WarningCount,
            job.DuplicateCount,
            job.FailedCount,
            job.ConflictCount,
            job.ResultSummaryJson,
            job.CorrelationId), cancellationToken);

    public Task PublishFailedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => eventPublisher.PublishAsync(new BackgroundJobFailedIntegrationEvent(
            job.Id,
            OwningService,
            job.OperationType.ToString(),
            job.Status.ToString(),
            job.CompletedAt ?? DateTimeOffset.UtcNow,
            job.ErrorSummary ?? "Catalog import job failed.",
            job.CorrelationId), cancellationToken);
}
