using System.Text.Json;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queueing;

public sealed class CatalogImportFunctionDispatcher(
    ICatalogImportJobExecutionGate executionGate,
    ICatalogImportJobProcessor processor,
    ILogger<CatalogImportFunctionDispatcher>? logger = null)
{
    private readonly ILogger<CatalogImportFunctionDispatcher> _logger =
        logger ?? NullLogger<CatalogImportFunctionDispatcher>.Instance;

    public async Task HandleAsync(
        string rawMessage,
        string queueBackend,
        CancellationToken cancellationToken = default)
    {
        var workItem = Deserialize(rawMessage);
        await HandleAsync(workItem, queueBackend, cancellationToken);
    }

    public async Task HandleAsync(
        CatalogImportQueueWorkItem workItem,
        string queueBackend,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(workItem.OperationType))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(workItem.OperationType));

        using var _ = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CatalogImportJobId"] = workItem.JobId,
            ["CatalogImportAttempt"] = workItem.Attempt,
            ["CorrelationId"] = workItem.CorrelationId,
            ["QueueBackend"] = queueBackend
        });

        var claimResult = await executionGate.TryClaimAsync(workItem, cancellationToken);
        if (!claimResult.Claimed)
        {
            _logger.LogInformation(
                "Skipped catalog import job {JobId} queued attempt {Attempt}: {SkipReason}",
                workItem.JobId,
                workItem.Attempt,
                claimResult.SkipReason);
            return;
        }

        await processor.ProcessClaimedQueuedJobAsync(workItem, cancellationToken);
    }

    private static CatalogImportQueueWorkItem Deserialize(string rawMessage)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(rawMessage));

        return JsonSerializer.Deserialize<CatalogImportQueueWorkItem>(
                rawMessage,
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(rawMessage));
    }
}
