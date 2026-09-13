namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ValidateCatalogImportBundleResultDto(
    Guid BundleId,
    string Status,
    int TotalProducts,
    int ReadyProducts,
    int BlockedProducts,
    int WarningCount,
    int DuplicateCount);
