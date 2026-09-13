namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryProvisioningResultDto(
    string Status,
    string SourceFingerprint,
    CategoryProvisioningSummaryDto Summary,
    IReadOnlyCollection<CategoryProvisioningCategoryResultDto> Results);
