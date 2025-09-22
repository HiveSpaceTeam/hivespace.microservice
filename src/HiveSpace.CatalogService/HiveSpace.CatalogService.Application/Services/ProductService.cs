using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Services.Base;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Services
{
    public class ProductService : BaseCRUDService<Product>, IProductService
    {
    }
}
