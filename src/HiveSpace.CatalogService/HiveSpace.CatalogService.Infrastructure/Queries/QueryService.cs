using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Application.Queries;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HiveSpace.CatalogService.Infrastructure.Queries
{
    public class QueryService(CatalogDbContext dbContext) : IQueryService
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
            return await _dbContext.Categories
                .Where(c => c.Id == categoryId)
                .SelectMany(c => c.CategoryAttributes)
                .Join(_dbContext.Attributes,
                    ca => ca.AttributeId,
                    a => a.Id,
                    (ca, a) => new AttributeViewModel
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
                        Values = _dbContext.AttributeValues
                            .Where(v => v.AttributeId == a.Id)
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
                            }).ToList()
                    })
                .ToListAsync();
        }
    }

}
