using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class CatalogImportQueueOutboxMessage
{
    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    public CatalogImportJobOperationType OperationType { get; private set; }
    public int Attempt { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset QueuedAt { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public Guid? SourceBundleId { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public DateTimeOffset? LastAttemptAt { get; private set; }
    public DateTimeOffset? DispatchedAt { get; private set; }
    public int FailureCount { get; private set; }
    public string? LastError { get; private set; }

    private CatalogImportQueueOutboxMessage()
    {
    }

    public static CatalogImportQueueOutboxMessage Create(CatalogImportJob job, string payloadJson, DateTimeOffset queuedAt)
    {
        if (job.Id == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(job.Id));
        if (job.Attempt < 1)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(job.Attempt));
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(PayloadJson));

        return new CatalogImportQueueOutboxMessage
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            OperationType = job.OperationType,
            Attempt = job.Attempt,
            CorrelationId = job.CorrelationId ?? job.Id.ToString("N"),
            QueuedAt = queuedAt,
            RequestedByUserId = job.RequestedByUserId,
            SourceBundleId = job.BundleId,
            PayloadJson = payloadJson
        };
    }

    public void MarkDispatched(DateTimeOffset dispatchedAt)
    {
        LastAttemptAt = dispatchedAt;
        DispatchedAt = dispatchedAt;
        LastError = null;
    }

    public void MarkDispatchFailed(string error, DateTimeOffset attemptedAt)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(error));

        LastAttemptAt = attemptedAt;
        FailureCount++;
        LastError = error.Trim();
    }
}
