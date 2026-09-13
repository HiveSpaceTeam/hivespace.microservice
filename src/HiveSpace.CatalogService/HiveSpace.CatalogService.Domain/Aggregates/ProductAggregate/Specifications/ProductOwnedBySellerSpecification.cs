using System.Linq.Expressions;
using HiveSpace.Domain.Shared.Specifications;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate.Specifications;

public class ProductOwnedByStoreSpecification(Guid storeId) : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
        => product => product.StoreId == storeId;
}
