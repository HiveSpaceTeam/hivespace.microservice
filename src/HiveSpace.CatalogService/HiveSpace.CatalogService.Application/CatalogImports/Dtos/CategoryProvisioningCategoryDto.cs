namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryProvisioningCategoryDto(
    string ExternalCategoryId,
    string? ExternalParentCategoryId,
    string Name,
    IReadOnlyCollection<string>? Path,
    string? ProductSetId,
    string? ImageUrl,
    string? ImageFileId,
    IReadOnlyDictionary<string, object>? Metadata);
