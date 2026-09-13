using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ExternalCategoryLink
{
    public Guid Id { get; private set; }
    public string SourceSystem { get; private set; } = string.Empty;
    public string ExternalCategoryId { get; private set; } = string.Empty;
    public string ExternalCategoryName { get; private set; } = string.Empty;
    public string? ExternalParentCategoryId { get; private set; }
    public string? PathJson { get; private set; }
    public int HiveSpaceCategoryId { get; private set; }
    public ImportedCategoryMappingStatus ProvisioningStatus { get; private set; }
    public string? ConflictReason { get; private set; }
    public string SourceFingerprint { get; private set; } = string.Empty;
    public Guid ProvisionedByUserId { get; private set; }
    public DateTimeOffset ProvisionedAt { get; private set; }

    private ExternalCategoryLink()
    {
    }

    public static ExternalCategoryLink Create(
        string sourceSystem,
        string externalCategoryId,
        string externalCategoryName,
        string? externalParentCategoryId,
        string? pathJson,
        int hiveSpaceCategoryId,
        string sourceFingerprint,
        Guid provisionedByUserId,
        ImportedCategoryMappingStatus status = ImportedCategoryMappingStatus.Mapped)
    {
        Require(sourceSystem, nameof(SourceSystem));
        Require(externalCategoryId, nameof(ExternalCategoryId));
        Require(externalCategoryName, nameof(ExternalCategoryName));
        Require(sourceFingerprint, nameof(SourceFingerprint));
        if (hiveSpaceCategoryId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(HiveSpaceCategoryId));
        if (provisionedByUserId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(ProvisionedByUserId));

        return new ExternalCategoryLink
        {
            Id = Guid.NewGuid(),
            SourceSystem = sourceSystem.Trim(),
            ExternalCategoryId = externalCategoryId.Trim(),
            ExternalCategoryName = externalCategoryName.Trim(),
            ExternalParentCategoryId = externalParentCategoryId,
            PathJson = pathJson,
            HiveSpaceCategoryId = hiveSpaceCategoryId,
            SourceFingerprint = sourceFingerprint.Trim(),
            ProvisioningStatus = status,
            ProvisionedByUserId = provisionedByUserId,
            ProvisionedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkMatched(
        string externalCategoryName,
        string? externalParentCategoryId,
        string? pathJson,
        int hiveSpaceCategoryId,
        string sourceFingerprint,
        Guid provisionedByUserId)
    {
        Require(externalCategoryName, nameof(ExternalCategoryName));
        Require(sourceFingerprint, nameof(SourceFingerprint));
        if (hiveSpaceCategoryId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(HiveSpaceCategoryId));
        if (provisionedByUserId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(ProvisionedByUserId));

        ExternalCategoryName = externalCategoryName.Trim();
        ExternalParentCategoryId = externalParentCategoryId;
        PathJson = pathJson;
        HiveSpaceCategoryId = hiveSpaceCategoryId;
        SourceFingerprint = sourceFingerprint.Trim();
        ProvisioningStatus = ImportedCategoryMappingStatus.Mapped;
        ConflictReason = null;
        ProvisionedByUserId = provisionedByUserId;
        ProvisionedAt = DateTimeOffset.UtcNow;
    }

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, field);
    }
}
