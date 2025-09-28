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
    }

}
