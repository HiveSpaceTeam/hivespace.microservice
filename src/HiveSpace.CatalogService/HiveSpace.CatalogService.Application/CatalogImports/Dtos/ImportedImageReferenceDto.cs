namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedImageReferenceDto(
    string Url,
    string Role,
    string? SourceImageId);
