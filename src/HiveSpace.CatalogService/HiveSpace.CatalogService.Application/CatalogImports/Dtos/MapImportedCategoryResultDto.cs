namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record MapImportedCategoryResultDto(
    Guid BundleId,
    string ExternalCategoryId,
    int HiveSpaceCategoryId,
    string MappingStatus,
    int AffectedProductCount);
