namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CategoryProvisioningCrawlDto(
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    string SourceFingerprint,
    string? CheckpointId);
