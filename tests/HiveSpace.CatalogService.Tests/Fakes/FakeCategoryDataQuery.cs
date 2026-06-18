using HiveSpace.CatalogService.Application.Categories;
using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Tests.Fakes;

public class FakeCategoryDataQuery(
    List<CategoryDto>? categories = null,
    List<AttributeDto>? attributes = null) : ICategoryDataQuery
{
    private readonly List<CategoryDto> _categories = categories ?? [];
    private readonly List<AttributeDto> _attributes = attributes ?? [];

    public Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_categories);

    public Task<List<CategoryDto>> GetHomepageCategoriesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_categories);

    public Task<List<AttributeDto>> GetAttributesByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
        => Task.FromResult(_attributes);
}
