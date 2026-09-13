namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportJobProgressDto(
    int Total,
    int Processed,
    int Created,
    int Matched,
    int Skipped,
    int Blocked,
    int Warning,
    int Duplicate,
    int Failed,
    int Conflict);
