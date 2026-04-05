using HiveSpace.CatalogService.Application.Products.Dtos;

namespace HiveSpace.CatalogService.Application.Products;

public interface IProductDataQuery
{
    Task<ProductDetailDto?> GetProductDetailAsync(int productId, CancellationToken cancellationToken = default);
}
