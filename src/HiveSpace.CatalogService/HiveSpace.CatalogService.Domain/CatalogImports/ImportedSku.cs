using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ImportedSku
{
    public Guid Id { get; private set; }
    public Guid ImportedProductId { get; private set; }
    public string ExternalSkuId { get; private set; } = string.Empty;
    public string? SkuNumber { get; private set; }
    public string VariantSelectionsJson { get; private set; } = "{}";
    public long? PriceAmount { get; private set; }
    public string? SourceRawPrice { get; private set; }
    public string? CurrencyCode { get; private set; }
    public string? ImageUrlsJson { get; private set; }
    public int? StockQuantity { get; private set; }
    public bool IsActiveCandidate { get; private set; }
    public ImportedProductReadinessStatus ReadinessStatus { get; private set; }

    private ImportedSku()
    {
    }

    public static ImportedSku Create(
        Guid importedProductId,
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
        if (string.IsNullOrWhiteSpace(externalSkuId))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedSku, nameof(ExternalSkuId));
        if (stockQuantity < 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedSku, nameof(StockQuantity));

        return new ImportedSku
        {
            Id = Guid.NewGuid(),
            ImportedProductId = importedProductId,
            ExternalSkuId = externalSkuId.Trim(),
            SkuNumber = skuNumber,
            VariantSelectionsJson = string.IsNullOrWhiteSpace(variantSelectionsJson) ? "{}" : variantSelectionsJson,
            PriceAmount = priceAmount,
            SourceRawPrice = sourceRawPrice,
            CurrencyCode = currencyCode,
            ImageUrlsJson = imageUrlsJson,
            StockQuantity = stockQuantity,
            IsActiveCandidate = isActiveCandidate,
            ReadinessStatus = priceAmount is > 0 && string.Equals(currencyCode, "VND", StringComparison.OrdinalIgnoreCase)
                ? ImportedProductReadinessStatus.Ready
                : ImportedProductReadinessStatus.Blocked
        };
    }
}
