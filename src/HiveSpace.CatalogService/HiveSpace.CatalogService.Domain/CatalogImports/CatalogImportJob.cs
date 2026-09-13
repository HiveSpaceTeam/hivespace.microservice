using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class CatalogImportJob
{
    public Guid Id { get; private set; }
    public CatalogImportJobOperationType OperationType { get; private set; }
    public CatalogImportJobStatus Status { get; private set; }
    public string SourceSystem { get; private set; } = string.Empty;
    public string? SourceFingerprint { get; private set; }
    public string? SourceFileName { get; private set; }
    public Guid? BundleId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset LastActivityAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int TotalCount { get; private set; }
    public int ProcessedCount { get; private set; }
    public int CreatedCount { get; private set; }
    public int MatchedCount { get; private set; }
    public int SkippedCount { get; private set; }
    public int BlockedCount { get; private set; }
    public int WarningCount { get; private set; }
    public int DuplicateCount { get; private set; }
    public int FailedCount { get; private set; }
    public int ConflictCount { get; private set; }
    public string? ResultSummaryJson { get; private set; }
    public string? ErrorSummary { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? RequestPayloadJson { get; private set; }

    private CatalogImportJob()
    {
    }

    public static CatalogImportJob Create(
        CatalogImportJobOperationType operationType,
        string sourceSystem,
        Guid requestedByUserId,
        string? sourceFingerprint = null,
        string? sourceFileName = null,
        Guid? bundleId = null,
        string? correlationId = null,
        string? requestPayloadJson = null)
    {
        if (requestedByUserId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(RequestedByUserId));
        if (string.IsNullOrWhiteSpace(sourceSystem))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(SourceSystem));

        var requestedAt = DateTimeOffset.UtcNow;

        return new CatalogImportJob
        {
            Id = Guid.NewGuid(),
            OperationType = operationType,
            Status = CatalogImportJobStatus.Pending,
            SourceSystem = sourceSystem.Trim(),
            SourceFingerprint = Normalize(sourceFingerprint),
            SourceFileName = Normalize(sourceFileName),
            BundleId = bundleId,
            RequestedByUserId = requestedByUserId,
            RequestedAt = requestedAt,
            LastActivityAt = requestedAt,
            CorrelationId = Normalize(correlationId),
            RequestPayloadJson = requestPayloadJson
        };
    }

    public void Requeue()
    {
        if (Status != CatalogImportJobStatus.Failed)
            throw new ConflictException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(Status));

        Status = CatalogImportJobStatus.Pending;
        LastActivityAt = DateTimeOffset.UtcNow;
        StartedAt = null;
        CompletedAt = null;
        TotalCount = 0;
        ProcessedCount = 0;
        CreatedCount = 0;
        MatchedCount = 0;
        SkippedCount = 0;
        BlockedCount = 0;
        WarningCount = 0;
        DuplicateCount = 0;
        FailedCount = 0;
        ConflictCount = 0;
        ResultSummaryJson = null;
        ErrorSummary = null;
    }

    public void Start()
    {
        if (Status != CatalogImportJobStatus.Pending)
            throw new ConflictException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(Status));

        Status = CatalogImportJobStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        LastActivityAt = StartedAt.Value;
        ErrorSummary = null;
    }

    public void UpdateProgress(
        int total,
        int processed,
        int created = 0,
        int matched = 0,
        int skipped = 0,
        int blocked = 0,
        int warnings = 0,
        int duplicates = 0,
        int failed = 0,
        int conflicts = 0)
    {
        if (Status != CatalogImportJobStatus.Running)
            throw new ConflictException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(Status));

        TotalCount = NonNegative(total, nameof(total));
        ProcessedCount = NonNegative(processed, nameof(processed));
        CreatedCount = NonNegative(created, nameof(created));
        MatchedCount = NonNegative(matched, nameof(matched));
        SkippedCount = NonNegative(skipped, nameof(skipped));
        BlockedCount = NonNegative(blocked, nameof(blocked));
        WarningCount = NonNegative(warnings, nameof(warnings));
        DuplicateCount = NonNegative(duplicates, nameof(duplicates));
        FailedCount = NonNegative(failed, nameof(failed));
        ConflictCount = NonNegative(conflicts, nameof(conflicts));
        LastActivityAt = DateTimeOffset.UtcNow;
    }

    public void Complete(string? resultSummaryJson = null, Guid? bundleId = null)
    {
        if (Status != CatalogImportJobStatus.Running)
            throw new ConflictException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(Status));

        Status = CatalogImportJobStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        LastActivityAt = CompletedAt.Value;
        ResultSummaryJson = resultSummaryJson;
        ErrorSummary = null;
        if (bundleId.HasValue)
            BundleId = bundleId;
    }

    public void Fail(string errorSummary)
    {
        if (Status != CatalogImportJobStatus.Running)
            throw new ConflictException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(Status));
        if (string.IsNullOrWhiteSpace(errorSummary))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(ErrorSummary));

        Status = CatalogImportJobStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        LastActivityAt = CompletedAt.Value;
        ErrorSummary = errorSummary.Trim();
    }

    private static int NonNegative(int value, string field)
    {
        if (value < 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, field);

        return value;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
