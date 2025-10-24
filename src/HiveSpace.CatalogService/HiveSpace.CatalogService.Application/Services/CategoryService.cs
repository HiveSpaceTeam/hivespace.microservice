using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Application.Queries;

namespace HiveSpace.CatalogService.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryDataQuery _queryService;
        public CategoryService(ICategoryDataQuery queryService)
        {
            _queryService = queryService;
        }

        public Task<List<CategoryViewModel>> GetCategoryAsync()
        {
            return _queryService.GetCategoryViewModelsAsync();
        }

        public Task<List<AttributeViewModel>> GetAttributesByCategoryIdAsync(Guid categoryId)
        {
            return _queryService.GetAttributesByCategoryIdAsync(categoryId);
        }
    }
}
