using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Application.Categories;

public interface ICategoryDataQuery
{
    Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<CategoryDto>> GetHomepageCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<AttributeDto>> GetAttributesByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);
}
