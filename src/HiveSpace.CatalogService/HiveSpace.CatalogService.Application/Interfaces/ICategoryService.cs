using HiveSpace.CatalogService.Application.Models.ViewModels;

namespace HiveSpace.CatalogService.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryViewModel>> GetCategoryAsync();
    Task<List<AttributeViewModel>> GetAttributesByCategoryIdAsync(int categoryId);
}
