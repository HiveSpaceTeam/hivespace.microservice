namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedSkuRequestDto(
    string ExternalSkuId,
    string? SkuNumber,
    IReadOnlyDictionary<string, string> VariantSelections,
    ImportedPriceRequestDto Price,
    int? StockQuantity,
    IReadOnlyCollection<string> ImageUrls);
