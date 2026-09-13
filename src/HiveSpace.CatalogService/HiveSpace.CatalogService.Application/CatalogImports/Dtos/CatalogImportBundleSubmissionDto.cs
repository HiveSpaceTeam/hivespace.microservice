namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportBundleSubmissionDto(
    Guid BundleId,
    string Status,
    string SourceFingerprint,
    CatalogImportSummaryCountsDto Summary);
