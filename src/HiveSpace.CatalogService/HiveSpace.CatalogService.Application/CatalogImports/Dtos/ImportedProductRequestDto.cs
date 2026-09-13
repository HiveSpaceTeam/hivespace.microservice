namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedProductRequestDto(
    string ExternalProductId,
    string ExternalSellerId,
    IReadOnlyCollection<string> ExternalCategoryIds,
    string? Url,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    IReadOnlyCollection<ImportedAttributeRequestDto> Attributes,
    IReadOnlyCollection<ImportedImageRequestDto> Images,
    IReadOnlyCollection<object> Variants,
    IReadOnlyCollection<ImportedSkuRequestDto> Skus);
