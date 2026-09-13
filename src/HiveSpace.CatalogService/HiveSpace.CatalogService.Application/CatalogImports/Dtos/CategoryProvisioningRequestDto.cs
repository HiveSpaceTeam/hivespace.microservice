namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryProvisioningRequestDto(
    string SchemaVersion,
    CategoryProvisioningSourceDto Source,
    CategoryProvisioningCrawlDto Crawl,
    IReadOnlyCollection<CategoryProvisioningCategoryDto> Categories);
