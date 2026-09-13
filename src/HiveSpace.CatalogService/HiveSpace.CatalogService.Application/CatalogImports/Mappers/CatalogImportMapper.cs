using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Domain.CatalogImports;
using System.Text.Json;

namespace HiveSpace.CatalogService.Application.CatalogImports.Mappers;

public static class CatalogImportMapper
{
    private static readonly JsonSerializerOptions MetadataJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static CatalogImportBundleSubmissionDto ToSubmissionDto(CatalogImportBundle bundle)
        => new(bundle.Id, bundle.Status.ToString(), bundle.SourceFingerprint, ToSummaryCountsDto(bundle));

    public static CatalogImportBundleSummaryDto ToSummaryDto(CatalogImportBundle bundle)
        => new(
            bundle.Id,
            bundle.Status.ToString(),
            new CatalogImportSourceDto(
                bundle.SourceSystem,
                bundle.SourceType,
                bundle.SourceValue,
                bundle.SourceUrl),
            new CatalogImportCrawlDto(
                bundle.CrawledAt,
                bundle.CrawledAt,
                bundle.SourceFingerprint,
                bundle.CheckpointId),
            bundle.SourceFileName,
            ToSummaryCountsDto(bundle),
            bundle.SubmittedByUserId.ToString(),
            bundle.SubmittedAt);

    public static CatalogImportBundleDetailDto ToDetailDto(CatalogImportBundle bundle)
        => new(
            ToSummaryDto(bundle),
            bundle.Sellers.Select(ToSellerDto).ToList(),
            bundle.SellerOwnershipLinks.Select(ToSellerOwnershipLinkDto).ToList(),
            bundle.CategoryMappings.Select(ToCategoryMappingDto).ToList(),
            bundle.Products.Select(ToProductDto).ToList(),
            bundle.DuplicateGroups.Select(ToDuplicateGroupDto).ToList(),
            bundle.ValidationIssues.Select(ToValidationIssueDto).ToList());

    private static CatalogImportSummaryCountsDto ToSummaryCountsDto(CatalogImportBundle bundle)
        => new(bundle.TotalProducts, bundle.ReadyProducts, bundle.BlockedProducts, bundle.WarningCount, bundle.DuplicateCount);

    public static ImportedSellerDto ToSellerDto(ImportedSeller seller)
        => new(
            seller.Id,
            seller.ExternalSellerId,
            seller.DisplayName,
            seller.ExternalSellerSlug,
            seller.SourceUrl,
            seller.LogoUrl,
            seller.ProvisioningStatus.ToString(),
            seller.HiveSpaceUserId,
            seller.HiveSpaceStoreId,
            seller.ConflictReason,
            ToExistingStoreCandidateDtos(seller),
            seller.ProvisioningStatus is Domain.CatalogImports.Enums.ImportedSellerProvisioningStatus.Conflict or Domain.CatalogImports.Enums.ImportedSellerProvisioningStatus.Failed);

    public static IReadOnlyCollection<ExistingStoreCandidateDto> ToExistingStoreCandidateDtos(ImportedSeller seller)
        => CreateExistingStoreCandidates(seller);

    public static ImportedCategoryMappingDto ToCategoryMappingDto(ImportedCategoryMapping category)
        => new(
            category.Id,
            category.ExternalCategoryId,
            category.ExternalParentCategoryId,
            category.HiveSpaceCategoryId?.ToString(),
            category.ExternalCategoryName,
            category.MappingStatus.ToString(),
            null);

    private static SellerOwnershipLinkDto ToSellerOwnershipLinkDto(SellerOwnershipLink link)
        => new(
            link.Id,
            link.ImportedSellerId,
            link.SourceSystem,
            link.ExternalSellerId,
            link.HiveSpaceUserId,
            link.HiveSpaceStoreId,
            link.LinkStatus.ToString(),
            link.CreatedByUserId,
            link.ApprovedByUserId,
            link.ApprovedAt,
            link.ApprovalReason);

    public static ImportedProductDto ToProductDto(ImportedProduct product)
        => new(
            product.Id,
            product.ExternalProductId,
            product.ExternalSellerId,
            Deserialize<IReadOnlyCollection<string>>(product.ExternalCategoryIdsJson, []),
            product.ExternalProductUrl,
            product.Title,
            product.Description,
            product.ThumbnailImageExternalUrl,
            product.ReadinessStatus.ToString(),
            product.ImportStatus.ToString(),
            product.Attributes.Select(ToAttributeDto).ToList(),
            product.Images.Select(ToImageDto).ToList(),
            product.Skus.Select(ToSkuDto).ToList());

    private static ImportedSkuDto ToSkuDto(ImportedSku sku)
        => new(
            sku.Id,
            sku.ExternalSkuId,
            sku.SkuNumber,
            Deserialize<IReadOnlyDictionary<string, string>>(sku.VariantSelectionsJson, new Dictionary<string, string>()),
            new ImportedSkuPriceDto(sku.PriceAmount, sku.CurrencyCode, sku.SourceRawPrice),
            sku.StockQuantity,
            Deserialize<IReadOnlyCollection<string>>(sku.ImageUrlsJson, []),
            sku.ReadinessStatus.ToString());

    private static ImportedAttributeDto ToAttributeDto(ImportedAttribute attribute)
        => new(attribute.ExternalAttributeName, attribute.ExternalAttributeValue, attribute.SourceAttributeId, attribute.SourceValueId);

    private static ImportedImageReferenceDto ToImageDto(ImportedImageReference image)
        => new(image.ExternalUrl, image.Role, image.SourceImageId);

    public static ImportValidationIssueDto ToValidationIssueDto(ImportValidationIssue issue)
        => new(
            issue.Id,
            issue.EntityType,
            issue.EntitySourceId,
            issue.Field,
            issue.Severity.ToString(),
            issue.ReasonCode,
            issue.Message,
            Deserialize<ImportValidationIssueMetadataDto?>(issue.MetadataJson, null, MetadataJsonOptions),
            issue.CreatedAt);

    public static ImportDuplicateGroupDto ToDuplicateGroupDto(ImportDuplicateGroup group)
        => new(
            group.Id,
            Deserialize<IReadOnlyCollection<string>>(group.ExternalProductIdsJson, []),
            null,
            group.DuplicateKey,
            group.ResolutionStatus.ToString());

    private static IReadOnlyCollection<ExistingStoreCandidateDto> CreateExistingStoreCandidates(ImportedSeller seller)
    {
        if (!seller.SuggestedHiveSpaceUserId.HasValue || !seller.SuggestedHiveSpaceStoreId.HasValue)
            return [];

        return
        [
            new ExistingStoreCandidateDto(
                seller.SuggestedHiveSpaceUserId.Value,
                seller.SuggestedHiveSpaceStoreId.Value,
                seller.DisplayName,
                seller.ConflictReason)
        ];
    }

    private static T Deserialize<T>(string? json, T fallback, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return fallback;

        return JsonSerializer.Deserialize<T>(json, options) ?? fallback;
    }
}
