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
            // Return only categories that have attributes linked to them via CategoryAttributes
            return await _dbContext.Categories
                .Where(c => c.CategoryAttributes.Any())
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

            // Step 2: Materialize attribute values, then group in-memory
            var attributeValues = await _dbContext.AttributeValues
                .Where(v => attributeIds.Contains(v.AttributeId))
                .AsNoTracking()
                .OrderBy(v => v.SortOrder)
                .Select(v => new AttributeValueViewModel(
                    v.Id,
                    v.AttributeId,
                    v.Name,
                    v.DisplayName,
                    v.ParentValueId,
                    v.IsActive,
                    v.SortOrder))
                .ToListAsync();

            var attributeValuesDict = attributeValues
                .GroupBy(v => v.AttributeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Step 3: Materialize attribute metadata, then compose records in-memory
            var attributeMetas = await _dbContext.Attributes
                .Where(a => attributeIds.Contains(a.Id))
                .AsNoTracking()
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Type.ValueType,
                    a.Type.InputType,
                    a.Type.IsMandatory,
                    a.Type.MaxValueCount,
                    a.IsActive,
                    a.CreatedAt,
                    a.UpdatedAt
                })
                .ToListAsync();

            var attributes = attributeMetas
                .Select(a => new AttributeViewModel(
                    a.Id,
                    a.Name,
                    a.ValueType,
                    a.InputType,
                    a.IsMandatory,
                    a.MaxValueCount,
                    a.IsActive,
                    a.CreatedAt,
                    a.UpdatedAt,
                    attributeValuesDict.TryGetValue(a.Id, out var vals) ? vals : new List<AttributeValueViewModel>()))
                .ToList();

            return attributes;
        }

    }

}
