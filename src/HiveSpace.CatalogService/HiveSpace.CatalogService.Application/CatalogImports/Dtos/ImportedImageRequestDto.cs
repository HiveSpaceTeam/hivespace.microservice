namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedImageRequestDto(
    string Url,
    string Role,
    string? SourceImageId);
