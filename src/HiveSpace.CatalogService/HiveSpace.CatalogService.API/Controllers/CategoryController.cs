using Asp.Versioning;
using HiveSpace.CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HiveSpace.CatalogService.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/categories")]
    [ApiController]
    [AllowAnonymous]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCategory()
        {
            var result = await categoryService.GetCategoryAsync();
            return Ok(result);
        }

        [HttpGet("{categoryId}/attributes")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAttributeByCategoryId(int categoryId)
        {
            var result = await categoryService.GetAttributesByCategoryIdAsync(categoryId);
            return Ok(result);
        }
    }
}
