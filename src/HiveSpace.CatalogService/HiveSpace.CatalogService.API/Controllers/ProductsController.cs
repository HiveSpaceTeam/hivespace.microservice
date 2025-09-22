using HiveSpace.CatalogService.API.Controllers.Base;
using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HiveSpace.CatalogService.API.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : BaseCrudController<Product, IProductService>
    {
        public ProductsController(IProductService service) : base(service)
        {
        }
    }
}
