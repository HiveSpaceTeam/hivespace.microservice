namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportCrawlDto(
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    string SourceFingerprint,
    string? CheckpointId);
