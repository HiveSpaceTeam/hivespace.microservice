namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportBundleRequestDto(
    string SchemaVersion,
    CatalogImportSourceDto Source,
    CatalogImportCrawlDto Crawl,
    IReadOnlyCollection<ImportedSellerRequestDto> Sellers,
    IReadOnlyCollection<ImportedCategoryRequestDto> Categories,
    IReadOnlyCollection<ImportedProductRequestDto> Products,
    IReadOnlyCollection<ImportValidationHintRequestDto> ValidationHints);
