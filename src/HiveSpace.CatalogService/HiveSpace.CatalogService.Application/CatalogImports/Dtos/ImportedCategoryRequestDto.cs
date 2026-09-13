namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedCategoryRequestDto(
    string ExternalCategoryId,
    string? ExternalParentCategoryId,
    string Name,
    IReadOnlyCollection<string>? Path);
