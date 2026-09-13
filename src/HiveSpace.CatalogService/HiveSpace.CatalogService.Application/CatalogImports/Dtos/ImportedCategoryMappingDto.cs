namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedCategoryMappingDto(
    Guid CategoryLinkId,
    string ExternalCategoryId,
    string? ExternalParentCategoryId,
    string? CategoryId,
    string? CategoryName,
    string Status,
    string? ConflictReason);
