namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedAttributeDto(
    string Name,
    string? Value,
    string? SourceAttributeId,
    string? SourceValueId);
