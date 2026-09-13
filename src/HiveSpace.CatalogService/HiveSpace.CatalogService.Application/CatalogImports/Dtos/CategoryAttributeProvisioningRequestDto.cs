namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryAttributeProvisioningRequestDto(
    string SchemaVersion,
    CategoryProvisioningSourceDto Source,
    CategoryProvisioningCrawlDto Crawl,
    IReadOnlyCollection<CategoryAttributeProvisioningCategoryDto> Categories);
