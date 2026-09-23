using System.Text.Json;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports.Queueing;

public sealed class CatalogImportJobScheduler(
    ICatalogImportBundleRepository repository,
    ILogger<CatalogImportJobScheduler> logger)
    : ICatalogImportJobScheduler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task ScheduleAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
    {
        var workItem = new CatalogImportQueueWorkItem(
            job.Id,
            job.OperationType,
            job.Attempt,
            job.CorrelationId ?? job.Id.ToString("N"),
            DateTimeOffset.UtcNow,
            job.RequestedByUserId,
            job.BundleId);

        logger.LogInformation(
            "Recording catalog import job {JobId} attempt {Attempt} in the queue outbox",
            workItem.JobId,
            workItem.Attempt);

        var payload = JsonSerializer.Serialize(workItem, JsonOptions);
        var outboxMessage = CatalogImportQueueOutboxMessage.Create(job, payload, workItem.QueuedAt);
        repository.AddQueueOutboxMessage(outboxMessage);
        await Task.CompletedTask;
    }
}
