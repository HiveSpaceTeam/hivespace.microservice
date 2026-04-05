using HiveSpace.CatalogService.Application.Categories;
using HiveSpace.CatalogService.Application.Categories.Dtos;
using HiveSpace.CatalogService.Application.Interfaces;

namespace HiveSpace.CatalogService.Application.Services;

public class CategoryService(ICategoryDataQuery categoryDataQuery) : ICategoryService
{
    public Task<List<CategoryDto>> GetCategoryAsync()
        => categoryDataQuery.GetCategoriesAsync();

    public Task<List<CategoryDto>> GetHomepageCategoriesAsync()
        => categoryDataQuery.GetHomepageCategoriesAsync();

    public Task<List<AttributeDto>> GetAttributesByCategoryIdAsync(int categoryId)
        => categoryDataQuery.GetAttributesByCategoryIdAsync(categoryId);
}
