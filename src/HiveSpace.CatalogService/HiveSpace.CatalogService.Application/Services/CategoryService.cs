using HiveSpace.CatalogService.Application.Helpers;
using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Application.Queries;

namespace HiveSpace.CatalogService.Application.Services;

public class CategoryService(IQueryService queryService, ICacheService redisService) : ICategoryService
{
    private readonly IQueryService _queryService = queryService;
    private readonly ICacheService _redisService = redisService;


    public async Task<List<CategoryViewModel>> GetCategoryAsync()
    {
        return await _redisService.GetOrCreateAsync(CacheKeys.Categories, _queryService.GetCategoryViewModelsAsync);
    }
}
