using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Application.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IQueryService _queryService;
        public CategoryService(IQueryService queryService)
        {
            _queryService = queryService;
        }

        public Task<List<CategoryViewModel>> GetCategoryAsync()
        {
            return _queryService.GetCategoryViewModelsAsync();
        }
    }
}
