using HiveSpace.CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HiveSpace.CatalogService.API.Controllers
{
    [Route("api/v1/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCategory()
        {
            var result = await _categoryService.GetCategoryAsync();
            return Ok(result);
        }

        [HttpGet("{categoryId}/attributes")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAttributeByCategoryId(Guid categoryId)
        {
            var result = await _categoryService.GetAttributesByCategoryIdAsync(categoryId);
            return Ok(result);
        }
    }
}
