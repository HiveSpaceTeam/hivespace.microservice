using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;

namespace HiveSpace.CatalogService.API.Controllers
{
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
        public async Task<IActionResult> Create([FromBody] Application.Models.Requests.ProductUpsertRequest request, CancellationToken cancellationToken)
        {
            var id = await _service.SaveProductAsync(request, cancellationToken);
            return StatusCode((int)HttpStatusCode.Created, id);
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] Application.Models.Requests.ProductUpsertRequest request, CancellationToken cancellationToken)
        {
            await _service.UpdateProductAsync(id, request, cancellationToken);
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
    }
}
