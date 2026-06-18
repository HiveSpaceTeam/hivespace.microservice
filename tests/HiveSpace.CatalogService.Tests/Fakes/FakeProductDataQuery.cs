using HiveSpace.CatalogService.Application.Products;
using HiveSpace.CatalogService.Application.Products.Dtos;

namespace HiveSpace.CatalogService.Tests.Fakes;

public class FakeProductDataQuery(ProductDetailDto? product = null) : IProductDataQuery
{
    public Task<ProductDetailDto?> GetProductDetailAsync(int productId, CancellationToken cancellationToken = default)
        => Task.FromResult(product);
}
