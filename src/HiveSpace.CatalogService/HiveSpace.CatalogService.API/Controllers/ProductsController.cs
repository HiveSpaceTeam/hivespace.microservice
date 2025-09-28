using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HiveSpace.CatalogService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public ProductsController(IProductService service) 
        {
        }
    }
}
