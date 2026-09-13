namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryProvisioningSourceDto(
    string System,
    string Type,
    string Value,
    string? Url);
