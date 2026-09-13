namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportDuplicateGroupDto(
    Guid GroupId,
    IReadOnlyCollection<string> ExternalProductIds,
    string? CanonicalExternalProductId,
    string ReasonCode,
    string ResolutionStatus);
