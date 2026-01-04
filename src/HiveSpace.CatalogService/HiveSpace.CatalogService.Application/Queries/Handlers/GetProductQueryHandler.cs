using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.CatalogService.Domain.Repositories.Domain;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.Queries.Handlers;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, Product>
{
    private readonly IProductRepository _productRepository;


    public GetProductQueryHandler(IProductRepository productService)
    {
        _productRepository = productService;
    }

    public async Task<Product> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetDetailByIdAsync(request.ProductId, cancellationToken)
              ?? throw new NotFoundException(CatalogErrorCode.ProductNotFound, nameof(Product));
        return product;
    }
}

