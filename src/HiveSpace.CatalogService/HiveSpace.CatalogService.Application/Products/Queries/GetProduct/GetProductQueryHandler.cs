using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProduct;

public class GetProductQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductQuery, Product>
{
    public async Task<Product> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        return await productRepository.GetDetailByIdAsync(request.ProductId, true, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.ProductNotFound, nameof(Product));
    }
}
