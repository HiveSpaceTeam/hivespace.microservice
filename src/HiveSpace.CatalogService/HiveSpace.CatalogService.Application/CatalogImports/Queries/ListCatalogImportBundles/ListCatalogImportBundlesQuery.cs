using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportBundles;

public record ListCatalogImportBundlesQuery(int PageNumber, int PageSize)
    : IQuery<CatalogImportPagedResponseDto<CatalogImportBundleSummaryDto>>;
