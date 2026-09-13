namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryProvisioningCategoryResultDto(
    string ExternalCategoryId,
    int? CategoryId,
    string Status,
    string? ConflictReason);
