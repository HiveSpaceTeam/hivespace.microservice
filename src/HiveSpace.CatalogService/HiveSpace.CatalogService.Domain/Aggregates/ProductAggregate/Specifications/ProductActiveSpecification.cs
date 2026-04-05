using System.Linq.Expressions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Specifications;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate.Specifications;

public class ProductActiveSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
        => product => product.Status == ProductStatus.Available;
}
