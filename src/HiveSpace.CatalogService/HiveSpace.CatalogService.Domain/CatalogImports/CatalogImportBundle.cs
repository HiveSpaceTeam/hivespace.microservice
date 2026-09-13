using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class CatalogImportBundle : AggregateRoot<Guid>
{
    private readonly List<ImportedSeller> _sellers = [];
    private readonly List<ImportedCategoryMapping> _categoryMappings = [];
    private readonly List<ImportedProduct> _products = [];
    private readonly List<ImportValidationIssue> _validationIssues = [];
    private readonly List<ImportDuplicateGroup> _duplicateGroups = [];
    private readonly List<SellerOwnershipLink> _sellerOwnershipLinks = [];

    public string SchemaVersion { get; private set; } = string.Empty;
    public string SourceSystem { get; private set; } = string.Empty;
    public string SourceType { get; private set; } = string.Empty;
    public string SourceValue { get; private set; } = string.Empty;
    public string? SourceUrl { get; private set; }
    public string SourceFingerprint { get; private set; } = string.Empty;
    public string? SourceFileName { get; private set; }
    public string? CheckpointId { get; private set; }
    public DateTimeOffset CrawledAt { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public Guid SubmittedByUserId { get; private set; }
    public CatalogImportBundleStatus Status { get; private set; }
    public int TotalProducts { get; private set; }
    public int ReadyProducts { get; private set; }
    public int BlockedProducts { get; private set; }
    public int WarningCount { get; private set; }
    public int DuplicateCount { get; private set; }

    public IReadOnlyCollection<ImportedSeller> Sellers => _sellers.AsReadOnly();
    public IReadOnlyCollection<ImportedCategoryMapping> CategoryMappings => _categoryMappings.AsReadOnly();
    public IReadOnlyCollection<ImportedCategoryMapping> CategoryLinks => _categoryMappings.AsReadOnly();
    public IReadOnlyCollection<ImportedProduct> Products => _products.AsReadOnly();
    public IReadOnlyCollection<ImportValidationIssue> ValidationIssues => _validationIssues.AsReadOnly();
    public IReadOnlyCollection<ImportDuplicateGroup> DuplicateGroups => _duplicateGroups.AsReadOnly();
    public IReadOnlyCollection<SellerOwnershipLink> SellerOwnershipLinks => _sellerOwnershipLinks.AsReadOnly();

    private CatalogImportBundle()
    {
    }

    public static CatalogImportBundle Create(
        string schemaVersion,
        string sourceSystem,
        string sourceType,
        string sourceValue,
        string sourceFingerprint,
        DateTimeOffset crawledAt,
        Guid submittedByUserId,
        string? sourceUrl = null,
        string? sourceFileName = null,
        string? checkpointId = null)
    {
        Require(schemaVersion, nameof(SchemaVersion));
        Require(sourceSystem, nameof(SourceSystem));
        Require(sourceType, nameof(SourceType));
        Require(sourceValue, nameof(SourceValue));
        Require(sourceFingerprint, nameof(SourceFingerprint));
        if (submittedByUserId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(SubmittedByUserId));

        return new CatalogImportBundle
        {
            Id = Guid.NewGuid(),
            SchemaVersion = schemaVersion.Trim(),
            SourceSystem = sourceSystem.Trim(),
            SourceType = sourceType.Trim(),
            SourceValue = sourceValue.Trim(),
            SourceUrl = sourceUrl,
            SourceFingerprint = sourceFingerprint.Trim(),
            SourceFileName = Normalize(sourceFileName),
            CheckpointId = checkpointId,
            CrawledAt = crawledAt,
            SubmittedAt = DateTimeOffset.UtcNow,
            SubmittedByUserId = submittedByUserId,
            Status = CatalogImportBundleStatus.Submitted
        };
    }

    public ImportedSeller AddSeller(
        string externalSellerId,
        string displayName,
        string? externalSellerSlug,
        string? sourceUrl,
        string? metadataJson,
        string logoUrl)
    {
        var seller = ImportedSeller.Create(Id, externalSellerId, displayName, externalSellerSlug, sourceUrl, metadataJson, logoUrl);
        _sellers.Add(seller);
        return seller;
    }

    public ImportedCategoryMapping AddCategory(
        string externalCategoryId,
        string? externalParentCategoryId,
        string externalCategoryName,
        string? pathJson)
    {
        var category = ImportedCategoryMapping.Create(Id, externalCategoryId, externalParentCategoryId, externalCategoryName, pathJson);
        _categoryMappings.Add(category);
        return category;
    }

    public ImportedCategoryMapping AddCategoryLink(
        string externalCategoryId,
        string? externalParentCategoryId,
        string externalCategoryName,
        int hiveSpaceCategoryId,
        string? pathJson = null)
    {
        var category = AddCategory(externalCategoryId, externalParentCategoryId, externalCategoryName, pathJson);
        category.MapToCategory(hiveSpaceCategoryId, SubmittedByUserId);
        return category;
    }

    public ImportedProduct AddProduct(
        string externalProductId,
        string externalSellerId,
        string title,
        IReadOnlyCollection<string> externalCategoryIds,
        string? externalProductUrl,
        string? description,
        string? thumbnailImageExternalUrl)
    {
        var product = ImportedProduct.Create(
            Id,
            externalProductId,
            externalSellerId,
            title,
            externalCategoryIds,
            externalProductUrl,
            description,
            thumbnailImageExternalUrl);
        _products.Add(product);
        return product;
    }

    public ImportValidationIssue AddValidationIssue(
        string entityType,
        string entitySourceId,
        string? field,
        ImportValidationSeverity severity,
        string reasonCode,
        string message,
        string? metadataJson = null)
    {
        var issue = ImportValidationIssue.Create(Id, entityType, entitySourceId, field, severity, reasonCode, message, metadataJson);
        _validationIssues.Add(issue);
        return issue;
    }

    public ImportDuplicateGroup AddDuplicateGroup(
        string duplicateKey,
        IReadOnlyCollection<string> externalProductIds,
        IReadOnlyCollection<Guid> memberImportedProductIds)
    {
        var group = ImportDuplicateGroup.Create(Id, duplicateKey, externalProductIds, memberImportedProductIds);
        _duplicateGroups.Add(group);
        return group;
    }

    public SellerOwnershipLink ApproveSellerOwnership(
        Guid importedSellerId,
        Guid targetUserId,
        Guid targetStoreId,
        Guid approvedByUserId,
        string approvalReason)
    {
        var seller = _sellers.FirstOrDefault(x => x.Id == importedSellerId)
            ?? throw new NotFoundException(CatalogDomainErrorCode.InvalidImportedSeller, nameof(ImportedSeller));
        if (_sellerOwnershipLinks.Any(x =>
                string.Equals(x.SourceSystem, SourceSystem, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.ExternalSellerId, seller.ExternalSellerId, StringComparison.OrdinalIgnoreCase)
                && x.LinkStatus == ImportedSellerProvisioningStatus.Matched))
            throw new ConflictException(CatalogDomainErrorCode.InvalidImportedSeller, nameof(SellerOwnershipLink));

        var link = SellerOwnershipLink.CreateApproved(
            Id,
            SourceSystem,
            seller.ExternalSellerId,
            seller.Id,
            targetUserId,
            targetStoreId,
            approvedByUserId,
            approvalReason);

        _sellerOwnershipLinks.Add(link);
        seller.MarkProvisioned(targetUserId, targetStoreId, created: false);
        return link;
    }

    public void ReplaceValidationIssues(IEnumerable<ImportValidationIssue> issues)
    {
        _validationIssues.Clear();
        _validationIssues.AddRange(issues);
        RefreshSummary();
        Status = BlockedProducts > 0 ? CatalogImportBundleStatus.NeedsAttention : CatalogImportBundleStatus.ReadyToImport;
    }

    public void RefreshSummary()
    {
        var blockingProductIds = _validationIssues
            .Where(i => i.Severity == ImportValidationSeverity.Blocking)
            .Select(i => i.EntitySourceId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var product in _products)
        {
            if (product.ImportStatus == ImportedProductImportStatus.Imported)
            {
                product.SetReadiness(ImportedProductReadinessStatus.Imported);
                continue;
            }

            var hasBlockingIssue = blockingProductIds.Contains(product.ExternalProductId)
                || product.Skus.Any(sku => blockingProductIds.Contains(sku.ExternalSkuId));
            var hasWarningIssue = _validationIssues.Any(i =>
                i.Severity == ImportValidationSeverity.Warning
                && (string.Equals(i.EntitySourceId, product.ExternalProductId, StringComparison.OrdinalIgnoreCase)
                    || product.Skus.Any(s => string.Equals(i.EntitySourceId, s.ExternalSkuId, StringComparison.OrdinalIgnoreCase))));

            product.SetReadiness(hasBlockingIssue
                ? ImportedProductReadinessStatus.Blocked
                : hasWarningIssue ? ImportedProductReadinessStatus.Warning : ImportedProductReadinessStatus.Ready);
        }

        TotalProducts = _products.Count;
        BlockedProducts = _products.Count(p => p.ReadinessStatus == ImportedProductReadinessStatus.Blocked);
        ReadyProducts = _products.Count(p => p.ReadinessStatus is ImportedProductReadinessStatus.Ready or ImportedProductReadinessStatus.Warning);
        WarningCount = _validationIssues.Count(i => i.Severity == ImportValidationSeverity.Warning);
        DuplicateCount = _duplicateGroups.Count;
        if (Status is CatalogImportBundleStatus.Imported or CatalogImportBundleStatus.PartiallyImported)
            return;

        Status = BlockedProducts > 0 ? CatalogImportBundleStatus.NeedsAttention : CatalogImportBundleStatus.Submitted;
    }

    public void RefreshImportStatus()
    {
        var imported = _products.Count(p => p.ImportStatus == ImportedProductImportStatus.Imported);
        if (imported == 0)
            return;

        Status = imported == _products.Count ? CatalogImportBundleStatus.Imported : CatalogImportBundleStatus.PartiallyImported;
    }

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportBundle, field);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
