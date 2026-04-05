using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoryAsync();
    Task<List<CategoryDto>> GetHomepageCategoriesAsync();
    Task<List<AttributeDto>> GetAttributesByCategoryIdAsync(int categoryId);
}
