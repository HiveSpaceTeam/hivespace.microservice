using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using System.Text.Json;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ImportedProduct
{
    private readonly List<ImportedSku> _skus = [];
    private readonly List<ImportedAttribute> _attributes = [];
    private readonly List<ImportedImageReference> _images = [];

    public Guid Id { get; private set; }
    public Guid BundleId { get; private set; }
    public string ExternalProductId { get; private set; } = string.Empty;
    public string? ExternalProductUrl { get; private set; }
    public string ExternalSellerId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ExternalCategoryIdsJson { get; private set; } = "[]";
    public string? ThumbnailImageExternalUrl { get; private set; }
    public ImportedProductReadinessStatus ReadinessStatus { get; private set; }
    public ImportedProductImportStatus ImportStatus { get; private set; }
    public int? ImportedProductId { get; private set; }

    public IReadOnlyCollection<ImportedSku> Skus => _skus.AsReadOnly();
    public IReadOnlyCollection<ImportedAttribute> Attributes => _attributes.AsReadOnly();
    public IReadOnlyCollection<ImportedImageReference> Images => _images.AsReadOnly();
    public IReadOnlyCollection<string> ExternalCategoryIds
        => JsonSerializer.Deserialize<IReadOnlyCollection<string>>(ExternalCategoryIdsJson) ?? [];

    private ImportedProduct()
    {
    }

    public static ImportedProduct Create(
        Guid bundleId,
        string externalProductId,
        string externalSellerId,
        string title,
        IReadOnlyCollection<string> externalCategoryIds,
        string? externalProductUrl,
        string? description,
        string? thumbnailImageExternalUrl)
    {
        if (string.IsNullOrWhiteSpace(externalProductId))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedProduct, nameof(ExternalProductId));
        if (string.IsNullOrWhiteSpace(externalSellerId))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedProduct, nameof(ExternalSellerId));
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedProduct, nameof(Title));
        if (externalCategoryIds.Count == 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedProduct, nameof(ExternalCategoryIdsJson));

        return new ImportedProduct
        {
            Id = Guid.NewGuid(),
            BundleId = bundleId,
            ExternalProductId = externalProductId.Trim(),
            ExternalSellerId = externalSellerId.Trim(),
            Title = title.Trim(),
            ExternalCategoryIdsJson = JsonSerializer.Serialize(externalCategoryIds),
            ExternalProductUrl = externalProductUrl,
            Description = description,
            ThumbnailImageExternalUrl = thumbnailImageExternalUrl,
            ReadinessStatus = ImportedProductReadinessStatus.Ready,
            ImportStatus = ImportedProductImportStatus.NotImported
        };
    }

    public ImportedSku AddSku(
        string externalSkuId,
        string? skuNumber,
        string variantSelectionsJson,
        long? priceAmount,
        string? sourceRawPrice,
        string? currencyCode,
        string? imageUrlsJson,
        int? stockQuantity,
        bool isActiveCandidate)
    {
        var sku = ImportedSku.Create(Id, externalSkuId, skuNumber, variantSelectionsJson, priceAmount, sourceRawPrice, currencyCode, imageUrlsJson, stockQuantity, isActiveCandidate);
        _skus.Add(sku);
        return sku;
    }

    public ImportedAttribute AddAttribute(string name, string? value, string? sourceAttributeId, string? sourceValueId)
    {
        var attribute = ImportedAttribute.Create(Id, name, value, sourceAttributeId, sourceValueId);
        _attributes.Add(attribute);
        return attribute;
    }

    public ImportedImageReference AddImage(string externalUrl, string role, string? sourceImageId, Guid? importedSkuId = null)
    {
        var image = ImportedImageReference.Create(Id, importedSkuId, externalUrl, role, sourceImageId);
        _images.Add(image);
        return image;
    }

    public void SetReadiness(ImportedProductReadinessStatus readinessStatus)
    {
        ReadinessStatus = readinessStatus;
    }

    public void MarkImported(int productId)
    {
        if (productId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedProduct, nameof(ImportedProductId));

        ImportedProductId = productId;
        ImportStatus = ImportedProductImportStatus.Imported;
        ReadinessStatus = ImportedProductReadinessStatus.Imported;
    }

    public void MarkSkipped()
    {
        ImportStatus = ImportedProductImportStatus.Skipped;
    }

    public void MarkFailed()
    {
        ImportStatus = ImportedProductImportStatus.Failed;
    }
}
