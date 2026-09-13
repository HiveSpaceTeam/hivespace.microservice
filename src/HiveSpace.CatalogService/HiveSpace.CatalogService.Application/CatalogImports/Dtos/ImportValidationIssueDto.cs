namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportValidationIssueDto(
    Guid IssueId,
    string EntityType,
    string EntitySourceId,
    string? Field,
    string Severity,
    string ReasonCode,
    string Message,
    ImportValidationIssueMetadataDto? Metadata,
    DateTimeOffset CreatedAt);

public record ImportValidationIssueMetadataDto(
    IReadOnlyCollection<string> MissingExternalCategoryIds);
