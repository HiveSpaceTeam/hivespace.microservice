namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryAttributeProvisioningAttributeDto(
    string SourceAttributeId,
    string Name,
    string InputType,
    bool IsRequired,
    IReadOnlyCollection<CategoryAttributeProvisioningAttributeValueDto> Values);
