using HiveSpace.CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HiveSpace.CatalogService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
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
    }
}
