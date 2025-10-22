using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;
using HiveSpace.CatalogService.Application.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Channels;

namespace HiveSpace.CatalogService.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) 
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> Create([FromBody] ProductUpsertRequest request, CancellationToken cancellationToken)
    {
        var id = await _service.SaveProductAsync(request, cancellationToken);
        return StatusCode((int)HttpStatusCode.Created, id);
    }

    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpsertRequest request, CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateProductAsync(id, request, cancellationToken);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetList([FromQuery] ProductSearchRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _service.GetProductsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var product = await _service.GetProductDetailAsync(id, cancellationToken);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteProductAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
