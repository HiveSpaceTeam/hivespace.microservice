namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportSourceDto(
    string System,
    string Type,
    string Value,
    string? Url);
