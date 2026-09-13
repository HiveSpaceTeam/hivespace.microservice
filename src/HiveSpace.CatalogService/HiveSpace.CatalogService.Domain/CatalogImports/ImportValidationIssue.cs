using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ImportValidationIssue
{
    public Guid Id { get; private set; }
    public Guid BundleId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string EntitySourceId { get; private set; } = string.Empty;
    public string? Field { get; private set; }
    public ImportValidationSeverity Severity { get; private set; }
    public string ReasonCode { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private ImportValidationIssue()
    {
    }

    public static ImportValidationIssue Create(
        Guid bundleId,
        string entityType,
        string entitySourceId,
        string? field,
        ImportValidationSeverity severity,
        string reasonCode,
        string message,
        string? metadataJson = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportValidationIssue, nameof(EntityType));
        if (string.IsNullOrWhiteSpace(entitySourceId))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportValidationIssue, nameof(EntitySourceId));
        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportValidationIssue, nameof(ReasonCode));

        return new ImportValidationIssue
        {
            Id = Guid.NewGuid(),
            BundleId = bundleId,
            EntityType = entityType.Trim(),
            EntitySourceId = entitySourceId.Trim(),
            Field = field,
            Severity = severity,
            ReasonCode = reasonCode.Trim(),
            Message = message,
            MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
