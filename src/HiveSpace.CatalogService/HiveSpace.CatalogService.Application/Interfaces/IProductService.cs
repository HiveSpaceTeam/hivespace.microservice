namespace HiveSpace.CatalogService.Application.Interfaces;

using HiveSpace.CatalogService.Application.Models.Requests;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

public interface IProductService
{
	Task<Guid> SaveProductAsync(ProductUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<PagingData> GetProductsAsync(ProductSearchRequestDto request, CancellationToken cancellationToken = default);
    Task<Product> GetProductDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(Guid id, ProductUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}
