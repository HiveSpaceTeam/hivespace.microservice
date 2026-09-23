using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports.Queueing;

public sealed class CatalogImportJobExecutionGate(ICatalogImportBundleRepository repository)
    : ICatalogImportJobExecutionGate
{
    public async Task<CatalogImportJobClaimResult> TryClaimAsync(
        CatalogImportQueueWorkItem workItem,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetJobByIdAsync(workItem.JobId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportJobNotFound, nameof(CatalogImportJob));

        if (job.OperationType != workItem.OperationType)
            return CatalogImportJobClaimResult.Skipped("OperationTypeMismatch");

        if (job.Attempt != workItem.Attempt)
            return CatalogImportJobClaimResult.Skipped("StaleAttempt");

        if (!job.CanProcessAttempt(workItem.Attempt))
            return CatalogImportJobClaimResult.Skipped($"Status:{job.Status}");

        var claimed = await repository.TryStartJobAsync(
            workItem.JobId,
            workItem.OperationType,
            workItem.Attempt,
            cancellationToken);

        return claimed
            ? CatalogImportJobClaimResult.Success()
            : CatalogImportJobClaimResult.Skipped("ClaimFailed");
    }
}
