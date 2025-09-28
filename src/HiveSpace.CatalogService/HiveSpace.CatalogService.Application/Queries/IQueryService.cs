using HiveSpace.CatalogService.Application.Models.ViewModels;

namespace HiveSpace.CatalogService.Application.Queries
{
    public interface IQueryService
    {
        Task<List<CategoryViewModel>> GetCategoryViewModelsAsync();
    }
}
