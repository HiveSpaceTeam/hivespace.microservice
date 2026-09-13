namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryProvisioningSummaryDto(
    int TotalCategories,
    int Created,
    int Matched,
    int Failed,
    int Conflict);
