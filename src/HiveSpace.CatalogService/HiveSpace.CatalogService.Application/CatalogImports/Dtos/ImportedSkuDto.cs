namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedSkuDto(
    Guid SkuId,
    string ExternalSkuId,
    string? SkuNumber,
    IReadOnlyDictionary<string, string> VariantSelections,
    ImportedSkuPriceDto Price,
    int? StockQuantity,
    IReadOnlyCollection<string> ImageUrls,
    string ReadinessStatus);

public record ImportedSkuPriceDto(
    long? Amount,
    string? CurrencyCode,
    string? SourceRawValue);
