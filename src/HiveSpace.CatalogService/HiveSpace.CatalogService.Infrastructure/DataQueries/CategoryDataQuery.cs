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
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<List<AttributeViewModel>> GetAttributesByCategoryIdAsync(Guid categoryId)
        {
            // Step 1: Get all attribute IDs linked to the category
            var attributeIds = await _dbContext.Categories
                .Where(c => c.Id == categoryId)
                .SelectMany(c => c.CategoryAttributes.Select(ca => ca.AttributeId))
                .Distinct()
                .ToListAsync();

            if (attributeIds.Count == 0)
                return [];

            var attributes = await _dbContext.Attributes
                .Where(a => attributeIds.Contains(a.Id))
                .Select(a => new AttributeViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    ValueType = a.Type.ValueType,
                    InputType = a.Type.InputType,
                    IsMandatory = a.Type.IsMandatory,
                    MaxValueCount = a.Type.MaxValueCount,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    Values = new List<AttributeValueViewModel>()
                })
                .ToListAsync();

            var attributeValues = await _dbContext.AttributeValues
                .Where(v => attributeIds.Contains(v.AttributeId))
                .OrderBy(v => v.SortOrder)
                .Select(v => new AttributeValueViewModel
                {
                    Id = v.Id,
                    AttributeId = v.AttributeId,
                    Name = v.Name,
                    DisplayName = v.DisplayName,
                    ParentValueId = v.ParentValueId,
                    IsActive = v.IsActive,
                    SortOrder = v.SortOrder
                })
                .ToListAsync();

            var attributeDict = attributes.ToDictionary(a => a.Id);
            foreach (var value in attributeValues)
            {
                if (attributeDict.TryGetValue(value.AttributeId, out var attribute))
                    attribute.Values.Add(value);
            }

            return attributes;
        }

    }

}
