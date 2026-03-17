using HiveSpace.CatalogService.Application.Models.ViewModels;

namespace HiveSpace.CatalogService.Application.DataQueries
{
    public interface IProductDataQuery
    {
        Task<ProductDetailViewModel?> GetProductDetailViewModelAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}