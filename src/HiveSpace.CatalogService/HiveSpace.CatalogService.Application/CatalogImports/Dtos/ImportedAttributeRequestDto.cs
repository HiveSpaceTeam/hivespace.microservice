namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedAttributeRequestDto(
    string Name,
    string? Value,
    string? SourceAttributeId,
    string? SourceValueId);
