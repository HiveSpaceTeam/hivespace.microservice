namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportBundleSummaryDto(
    Guid BundleId,
    string Status,
    CatalogImportSourceDto Source,
    CatalogImportCrawlDto Crawl,
    string? SourceFileName,
    CatalogImportSummaryCountsDto Summary,
    string? SubmittedBy,
    DateTimeOffset SubmittedAt);
