using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.GetCatalogImportJob;

public record GetCatalogImportJobQuery(Guid JobId) : IQuery<CatalogImportJobDto>;
