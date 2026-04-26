using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Products.Dtos;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProductDetail;

public class GetProductDetailQueryHandler(IProductDataQuery productDataQuery)
    : IQueryHandler<GetProductDetailQuery, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
    {
        return await productDataQuery.GetProductDetailAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.ProductNotFound, nameof(ProductDetailDto));
    }
}
