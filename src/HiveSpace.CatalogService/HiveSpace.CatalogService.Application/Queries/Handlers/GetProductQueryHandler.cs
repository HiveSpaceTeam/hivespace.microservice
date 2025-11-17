using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Queries.Handlers;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, Product>
{
    private readonly IProductService _productService;

    public GetProductQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public Task<Product> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        return _productService.GetProductDetailAsync(request.ProductId, cancellationToken);
    }
}

