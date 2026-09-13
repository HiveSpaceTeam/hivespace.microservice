namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportReadyProductsResultDto(
    Guid BundleId,
    int ImportedCount,
    int SkippedCount,
    int BlockedCount,
    int FailedCount);
