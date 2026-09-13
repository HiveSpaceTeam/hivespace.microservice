using HiveSpace.CatalogService.Domain.CatalogImports.Enums;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class SellerOwnershipLink
{
    public Guid Id { get; private set; }
    public Guid BundleId { get; private set; }
    public string SourceSystem { get; private set; } = string.Empty;
    public string ExternalSellerId { get; private set; } = string.Empty;
    public Guid ImportedSellerId { get; private set; }
    public Guid? HiveSpaceUserId { get; private set; }
    public Guid? HiveSpaceStoreId { get; private set; }
    public ImportedSellerProvisioningStatus LinkStatus { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public string? ApprovalReason { get; private set; }

    private SellerOwnershipLink()
    {
    }

    public static SellerOwnershipLink CreateApproved(
        Guid bundleId,
        string sourceSystem,
        string externalSellerId,
        Guid importedSellerId,
        Guid userId,
        Guid storeId,
        Guid approvedByUserId,
        string approvalReason)
    {
        if (bundleId == Guid.Empty)
            throw new HiveSpace.Domain.Shared.Exceptions.InvalidFieldException(Domain.Exceptions.CatalogDomainErrorCode.InvalidImportedSeller, nameof(BundleId));
        if (string.IsNullOrWhiteSpace(sourceSystem))
            throw new HiveSpace.Domain.Shared.Exceptions.InvalidFieldException(Domain.Exceptions.CatalogDomainErrorCode.InvalidImportedSeller, nameof(SourceSystem));
        if (string.IsNullOrWhiteSpace(externalSellerId))
            throw new HiveSpace.Domain.Shared.Exceptions.InvalidFieldException(Domain.Exceptions.CatalogDomainErrorCode.InvalidImportedSeller, nameof(ExternalSellerId));
        if (importedSellerId == Guid.Empty)
            throw new HiveSpace.Domain.Shared.Exceptions.InvalidFieldException(Domain.Exceptions.CatalogDomainErrorCode.InvalidImportedSeller, nameof(ImportedSellerId));
        if (userId == Guid.Empty)
            throw new HiveSpace.Domain.Shared.Exceptions.InvalidFieldException(Domain.Exceptions.CatalogDomainErrorCode.InvalidImportedSeller, nameof(HiveSpaceUserId));
        if (storeId == Guid.Empty)
            throw new HiveSpace.Domain.Shared.Exceptions.InvalidFieldException(Domain.Exceptions.CatalogDomainErrorCode.InvalidImportedSeller, nameof(HiveSpaceStoreId));
        if (approvedByUserId == Guid.Empty)
            throw new HiveSpace.Domain.Shared.Exceptions.InvalidFieldException(Domain.Exceptions.CatalogDomainErrorCode.InvalidImportedSeller, nameof(ApprovedByUserId));
        if (string.IsNullOrWhiteSpace(approvalReason))
            throw new HiveSpace.Domain.Shared.Exceptions.InvalidFieldException(Domain.Exceptions.CatalogDomainErrorCode.InvalidImportedSeller, nameof(ApprovalReason));

        var now = DateTimeOffset.UtcNow;
        return new SellerOwnershipLink
        {
            Id = Guid.NewGuid(),
            BundleId = bundleId,
            SourceSystem = sourceSystem.Trim(),
            ExternalSellerId = externalSellerId.Trim(),
            ImportedSellerId = importedSellerId,
            HiveSpaceUserId = userId,
            HiveSpaceStoreId = storeId,
            LinkStatus = ImportedSellerProvisioningStatus.Matched,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = approvedByUserId,
            ApprovedByUserId = approvedByUserId,
            ApprovedAt = now,
            ApprovalReason = approvalReason.Trim()
        };
    }
}
