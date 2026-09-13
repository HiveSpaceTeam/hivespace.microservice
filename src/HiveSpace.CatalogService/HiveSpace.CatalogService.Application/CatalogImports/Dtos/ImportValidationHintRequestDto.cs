namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportValidationHintRequestDto(
    string EntityType,
    string EntitySourceId,
    string? Field,
    string Severity,
    string ReasonCode,
    string Message);
