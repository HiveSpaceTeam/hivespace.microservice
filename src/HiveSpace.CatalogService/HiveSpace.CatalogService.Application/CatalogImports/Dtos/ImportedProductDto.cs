namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedProductDto(
    Guid ProductId,
    string ExternalProductId,
    string ExternalSellerId,
    IReadOnlyCollection<string> ExternalCategoryIds,
    string? Url,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    string ReadinessStatus,
    string ImportStatus,
    IReadOnlyCollection<ImportedAttributeDto> Attributes,
    IReadOnlyCollection<ImportedImageReferenceDto> Images,
    IReadOnlyCollection<ImportedSkuDto> Skus);
