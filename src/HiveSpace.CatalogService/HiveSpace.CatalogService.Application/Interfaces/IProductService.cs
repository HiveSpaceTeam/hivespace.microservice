using HiveSpace.CatalogService.Application.Interfaces.Base;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Interfaces;

public interface IProductService: IBaseCRUDService<Product>
{
}
