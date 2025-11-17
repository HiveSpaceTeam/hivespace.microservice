using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;

namespace HiveSpace.CatalogService.Application.Queries.Handlers;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PagingData>
{
    private readonly IProductService _productService;

    public GetProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public Task<PagingData> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return _productService.GetProductsAsync(request.Payload, cancellationToken);
    }
}

