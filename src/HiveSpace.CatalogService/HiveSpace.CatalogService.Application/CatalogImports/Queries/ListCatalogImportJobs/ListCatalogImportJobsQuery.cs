using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportJobs;

public record ListCatalogImportJobsQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    string? Status,
    string? OperationType,
    string? SourceSystem,
    Guid? BundleId,
    DateTimeOffset? RequestedFrom,
    DateTimeOffset? RequestedTo)
    : IQuery<CatalogImportPagedResponseDto<CatalogImportJobDto>>;
