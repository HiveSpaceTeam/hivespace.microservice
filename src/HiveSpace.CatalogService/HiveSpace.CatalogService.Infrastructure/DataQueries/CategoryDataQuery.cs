using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Application.Queries;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HiveSpace.CatalogService.Infrastructure.Queries
{
    public class CategoryDataQuery(CatalogDbContext dbContext) : ICategoryDataQuery
    {
        private readonly CatalogDbContext _dbContext = dbContext;

        public async Task<List<CategoryViewModel>> GetCategoryViewModelsAsync()
        {
            return await _dbContext.Categories
                .Select(c => new CategoryViewModel(
                    c.Id,
                    c.Name,
                    c.Name, // DisplayName - using Name for now
                    string.Empty // FileImageId - default empty for now
                ))
                .ToListAsync();
        }

        public async Task<List<AttributeViewModel>> GetAttributesByCategoryIdAsync(int categoryId)
        {
            // Step 1: Get all attribute IDs linked to the category
            var attributeIds = await _dbContext.Categories
                .Where(c => c.Id == categoryId)
                .SelectMany(c => c.CategoryAttributes.Select(ca => ca.AttributeId))
                .Distinct()
                .ToListAsync();

            if (attributeIds.Count == 0)
                return [];

            // Step 2: Get attribute values grouped by attribute ID
            var attributeValuesDict = await _dbContext.AttributeValues
                .Where(v => attributeIds.Contains(v.AttributeId))
                .OrderBy(v => v.SortOrder)
                .Select(v => new AttributeValueViewModel(
                    v.Id,
                    v.AttributeId,
                    v.Name,
                    v.DisplayName,
                    v.ParentValueId,
                    v.IsActive,
                    v.SortOrder
                ))
                .GroupBy(v => v.AttributeId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            // Step 3: Get attributes with their values
            var attributes = await _dbContext.Attributes
                .Where(a => attributeIds.Contains(a.Id))
                .Select(a => new AttributeViewModel(
                    a.Id,
                    a.Name,
                    a.Type.ValueType,
                    a.Type.InputType,
                    a.Type.IsMandatory,
                    a.Type.MaxValueCount,
                    a.IsActive,
                    a.CreatedAt,
                    a.UpdatedAt,
                    attributeValuesDict.ContainsKey(a.Id) ? attributeValuesDict[a.Id] : new List<AttributeValueViewModel>()
                ))
                .ToListAsync();

            return attributes;
        }

    }

}
