namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportSummaryCountsDto(
    int TotalProducts,
    int ReadyProducts,
    int BlockedProducts,
    int WarningCount,
    int DuplicateCount);
