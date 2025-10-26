using HiveSpace.CatalogService.Application.Models.ViewModels;

namespace HiveSpace.CatalogService.Application.Queries
{
    public interface ICategoryDataQuery
    {
        Task<List<CategoryViewModel>> GetCategoryViewModelsAsync();
        Task<List<AttributeViewModel>> GetAttributesByCategoryIdAsync(int categoryId);
    }
}
