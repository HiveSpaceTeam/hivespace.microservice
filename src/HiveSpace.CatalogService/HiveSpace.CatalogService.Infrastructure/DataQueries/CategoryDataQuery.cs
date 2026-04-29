using HiveSpace.CatalogService.Application.Categories;
using HiveSpace.CatalogService.Application.Categories.Dtos;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.DataQueries;

public class CategoryDataQuery(CatalogDbContext dbContext) : ICategoryDataQuery
{
    public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .Where(c => c.CategoryAttributes.Any())
            .Select(c => new CategoryDto(c.Id, c.Name, c.Name, string.Empty))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CategoryDto>> GetHomepageCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .Where(c => c.FilePath != null)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Name, c.FilePath!))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AttributeDto>> GetAttributesByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var attributeIds = await dbContext.Categories
            .Where(c => c.Id == categoryId)
            .SelectMany(c => c.CategoryAttributes.Select(ca => ca.AttributeId))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (attributeIds.Count == 0) return [];

        var attributeValues = await dbContext.AttributeValues
            .Where(v => attributeIds.Contains(v.AttributeId))
            .AsNoTracking()
            .OrderBy(v => v.SortOrder)
            .Select(v => new AttributeValueDto(v.Id, v.AttributeId, v.Name, v.DisplayName, v.ParentValueId, v.IsActive, v.SortOrder))
            .ToListAsync(cancellationToken);

        var attributeValuesDict = attributeValues
            .GroupBy(v => v.AttributeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var attributeMetas = await dbContext.Attributes
            .Where(a => attributeIds.Contains(a.Id))
            .AsNoTracking()
            .Select(a => new { a.Id, a.Name, a.Type.ValueType, a.Type.InputType, a.Type.IsMandatory, a.Type.MaxValueCount, a.IsActive, a.CreatedAt, a.UpdatedAt })
            .ToListAsync(cancellationToken);

        return attributeMetas
            .Select(a => new AttributeDto(
                a.Id, a.Name, a.ValueType, a.InputType, a.IsMandatory, a.MaxValueCount, a.IsActive, a.CreatedAt, a.UpdatedAt,
                attributeValuesDict.TryGetValue(a.Id, out var vals) ? vals : []))
            .ToList();
    }
}
