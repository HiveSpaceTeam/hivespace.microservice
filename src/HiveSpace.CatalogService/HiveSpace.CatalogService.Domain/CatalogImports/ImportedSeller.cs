using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ImportedSeller
{
    public Guid Id { get; private set; }
    public Guid BundleId { get; private set; }
    public string ExternalSellerId { get; private set; } = string.Empty;
    public string? ExternalSellerSlug { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? SourceUrl { get; private set; }
    public string LogoUrl { get; private set; } = string.Empty;
    public string? MetadataJson { get; private set; }
    public ImportedSellerProvisioningStatus ProvisioningStatus { get; private set; }
    public Guid? HiveSpaceUserId { get; private set; }
    public Guid? HiveSpaceStoreId { get; private set; }
    public string? ConflictReason { get; private set; }
    public Guid? SuggestedHiveSpaceStoreId { get; private set; }
    public Guid? SuggestedHiveSpaceUserId { get; private set; }
    public bool HasActiveOwnership => HiveSpaceUserId.HasValue && HiveSpaceStoreId.HasValue;

    private ImportedSeller()
    {
    }

    public static ImportedSeller Create(
        Guid bundleId,
        string externalSellerId,
        string displayName,
        string? externalSellerSlug,
        string? sourceUrl,
        string? metadataJson,
        string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(externalSellerId))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedSeller, nameof(ExternalSellerId));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedSeller, nameof(DisplayName));
        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedSeller, nameof(LogoUrl));

        return new ImportedSeller
        {
            Id = Guid.NewGuid(),
            BundleId = bundleId,
            ExternalSellerId = externalSellerId.Trim(),
            ExternalSellerSlug = externalSellerSlug,
            DisplayName = displayName.Trim(),
            SourceUrl = sourceUrl,
            LogoUrl = logoUrl.Trim(),
            MetadataJson = metadataJson,
            ProvisioningStatus = ImportedSellerProvisioningStatus.Unmatched
        };
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public void MarkCreateRequested()
    {
        ProvisioningStatus = ImportedSellerProvisioningStatus.CreateRequested;
        ConflictReason = null;
    }

    public void MarkProvisioned(
        Guid userId,
        Guid storeId,
        bool created)
    {
        if (userId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedSeller, nameof(HiveSpaceUserId));
        if (storeId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedSeller, nameof(HiveSpaceStoreId));

        HiveSpaceUserId = userId;
        HiveSpaceStoreId = storeId;
        ConflictReason = null;
        ProvisioningStatus = created
            ? ImportedSellerProvisioningStatus.Created
            : ImportedSellerProvisioningStatus.Matched;
    }

    public void MarkConflict(string reason)
    {
        ProvisioningStatus = ImportedSellerProvisioningStatus.Conflict;
        ConflictReason = string.IsNullOrWhiteSpace(reason) ? "ImportedSellerConflict" : reason.Trim();
    }

    public void MarkConflict(string reason, Guid? suggestedStoreId, Guid? suggestedUserId)
    {
        MarkConflict(reason);
        SuggestedHiveSpaceStoreId = suggestedStoreId;
        SuggestedHiveSpaceUserId = suggestedUserId;
    }

    public void MarkFailed(string reason)
    {
        ProvisioningStatus = ImportedSellerProvisioningStatus.Failed;
        ConflictReason = string.IsNullOrWhiteSpace(reason) ? "ImportedSellerProvisioningFailed" : reason.Trim();
    }
}
