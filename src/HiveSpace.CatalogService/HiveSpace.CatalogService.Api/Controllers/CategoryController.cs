using Asp.Versioning;
using HiveSpace.CatalogService.Application.Categories.Queries.GetAttributesByCategoryId;
using HiveSpace.CatalogService.Application.Categories.Queries.GetCategories;
using HiveSpace.CatalogService.Application.Categories.Queries.GetHomepageCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HiveSpace.CatalogService.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
[ApiController]
[AllowAnonymous]
public class CategoryController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetCategory()
    {
        var result = await sender.Send(new GetCategoriesQuery());
        return Ok(result);
    }

    [HttpGet("homepage")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetHomepageCategories()
    {
        var result = await sender.Send(new GetHomepageCategoriesQuery());
        return Ok(result);
    }

    [HttpGet("{categoryId}/attributes")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAttributeByCategoryId(int categoryId)
    {
        var result = await sender.Send(new GetAttributesByCategoryIdQuery(categoryId));
        return Ok(result);
    }
}
