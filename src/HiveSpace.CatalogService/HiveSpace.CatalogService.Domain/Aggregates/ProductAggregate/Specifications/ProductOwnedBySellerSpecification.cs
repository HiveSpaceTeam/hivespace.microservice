using System.Linq.Expressions;
using HiveSpace.Domain.Shared.Specifications;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate.Specifications;

public class ProductOwnedBySellerSpecification(Guid sellerId) : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
        => product => product.SellerId == sellerId;
}
