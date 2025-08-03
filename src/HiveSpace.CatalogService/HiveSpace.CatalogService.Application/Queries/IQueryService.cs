using HiveSpace.Application.Models.ViewModels;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;
using HiveSpace.CatalogService.Application.Models.ViewModels;

namespace HiveSpace.CatalogService.Application.Queries;

public interface IQueryService
{
    Task<List<CategoryViewModel>> GetCategoryViewModelsAsync();
    Task<List<ProductSearchViewModel>> GetProductSearchViewModelAsync(ProductSearchRequestDto param);
    Task<List<ProductHomeViewModel>> GetProductHomeViewModelAsync(ProductHomeRequestDto param);
    Task<List<ProductsByCategoryDto>> GetProductSearchViewModelAsync(int categoryId);
    Task<List<ProductCategoryViewModel>> GetCategoryTree(int categoryId);
}
