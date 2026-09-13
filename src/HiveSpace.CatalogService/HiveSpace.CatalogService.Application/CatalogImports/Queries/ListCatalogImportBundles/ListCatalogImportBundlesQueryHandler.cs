using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportBundles;

public class ListCatalogImportBundlesQueryHandler(ICatalogImportBundleRepository repository)
    : IQueryHandler<ListCatalogImportBundlesQuery, CatalogImportPagedResponseDto<CatalogImportBundleSummaryDto>>
{
    public async Task<CatalogImportPagedResponseDto<CatalogImportBundleSummaryDto>> Handle(
        ListCatalogImportBundlesQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (bundles, total) = await repository.ListBundlesAsync(pageNumber, pageSize, cancellationToken);

        return new CatalogImportPagedResponseDto<CatalogImportBundleSummaryDto>(
            bundles.Select(CatalogImportMapper.ToSummaryDto).ToList(),
            CatalogImportPaginationDto.Create(pageNumber, pageSize, total));
    }
}
