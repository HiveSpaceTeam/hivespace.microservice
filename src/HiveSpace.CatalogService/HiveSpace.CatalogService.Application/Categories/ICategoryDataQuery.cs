using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Application.Categories;

public interface ICategoryDataQuery
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<List<CategoryDto>> GetHomepageCategoriesAsync();
    Task<List<AttributeDto>> GetAttributesByCategoryIdAsync(int categoryId);
}
