using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.GetCatalogImportBundleDetail;

public record GetCatalogImportBundleDetailQuery(Guid BundleId) : IQuery<CatalogImportBundleSummaryDto>;
