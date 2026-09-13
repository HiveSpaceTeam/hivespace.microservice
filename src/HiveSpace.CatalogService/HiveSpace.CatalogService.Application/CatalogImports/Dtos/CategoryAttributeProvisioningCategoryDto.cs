namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryAttributeProvisioningCategoryDto(
    string ExternalCategoryId,
    string? ProductSetId,
    IReadOnlyCollection<CategoryAttributeProvisioningAttributeDto> Attributes);
