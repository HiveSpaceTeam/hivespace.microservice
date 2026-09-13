using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportBundleSection;

public record ListCatalogImportBundleSectionQuery(
    Guid BundleId,
    string Section,
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    string? Status,
    string? SellerId,
    string? Severity,
    string? EntityType,
    string? ReasonCode)
    : IQuery<object>;
