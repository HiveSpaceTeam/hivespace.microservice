namespace HiveSpace.CatalogService.Application.Interfaces;

using HiveSpace.CatalogService.Application.Models.Requests;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;

public interface IProductService
{
	Task<Guid> SaveProductAsync(ProductUpsertRequest request, CancellationToken cancellationToken = default);
    Task<PagingData> GetProductsAsync(ProductSearchRequestDto request, CancellationToken cancellationToken = default);
    Task<object?> GetProductDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateProductAsync(Guid id, ProductUpsertRequest request, CancellationToken cancellationToken = default);
}
