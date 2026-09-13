using System.Text.Json.Nodes;

namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportJobDto(
    Guid JobId,
    string OperationType,
    string Status,
    string SourceSystem,
    string? SourceFingerprint,
    string? SourceFileName,
    Guid? BundleId,
    Guid RequestedByUserId,
    DateTimeOffset RequestedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    CatalogImportJobProgressDto Progress,
    JsonNode? ResultSummary,
    string? ErrorSummary);
