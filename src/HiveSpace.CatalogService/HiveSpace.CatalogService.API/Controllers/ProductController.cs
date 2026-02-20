using HiveSpace.CatalogService.Application.Commands;
using HiveSpace.CatalogService.Application.Queries;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;
using HiveSpace.CatalogService.Application.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using MediatR;
using HiveSpace.Infrastructure.Authorization;
using Asp.Versioning;

namespace HiveSpace.CatalogService.Api.Controllers;

[Route("api/v{version:apiVersion}/products")]
[ApiVersion("1.0")]
[ApiController]
[RequireSeller]
public class ProductController(IMediator mediator) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> Create([FromBody] ProductUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateProductCommand(request), cancellationToken);
        return StatusCode((int)HttpStatusCode.Created, id);
    }

    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(new UpdateProductCommand(id, request), cancellationToken);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetList([FromQuery] ProductSearchRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductsQuery(request), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetProductQuery(id), cancellationToken);
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteProductCommand(id), cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
