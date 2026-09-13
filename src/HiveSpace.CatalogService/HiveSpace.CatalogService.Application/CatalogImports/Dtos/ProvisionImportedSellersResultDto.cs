namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ProvisionImportedSellersResultDto(
    Guid BundleId,
    int CreatedCount,
    int MatchedCount,
    int SkippedCount,
    int ConflictCount,
    int FailedCount);
