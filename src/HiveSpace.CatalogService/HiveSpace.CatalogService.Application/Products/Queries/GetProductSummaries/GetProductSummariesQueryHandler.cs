using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Products.Dtos;
using HiveSpace.CatalogService.Application.Products.Mappers;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProductSummaries;

public class GetProductSummariesQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductSummariesQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<PagedResult<ProductSummaryDto>> Handle(GetProductSummariesQuery request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var (items, total) = await productRepository.GetSummariesPagedAsync(
            payload.Keyword ?? string.Empty, payload.PageIndex, payload.PageSize, payload.Sort, cancellationToken);

        var dtos = items.Select(p => p.ToSummaryDto()).ToList();

        return new PagedResult<ProductSummaryDto>(dtos, payload.PageIndex + 1, payload.PageSize, total);
    }
}
