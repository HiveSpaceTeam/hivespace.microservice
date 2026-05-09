using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IProductRepository productRepository, IUserContext userContext)
    : IQueryHandler<GetProductsQuery, PagedResult<Product>>
{
    public async Task<PagedResult<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var (items, total) = await productRepository.GetPagedAsync(
            payload.Keyword ?? string.Empty, payload.Page, payload.PageSize, payload.Sort,
            userContext.UserId, cancellationToken);

        return new PagedResult<Product>(items, payload.Page + 1, payload.PageSize, total);
    }
}
