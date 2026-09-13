namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportJobSubmissionDto(
    Guid JobId,
    string Status,
    string OperationType,
    string? SourceFingerprint,
    string? SourceFileName,
    Guid? BundleId);
