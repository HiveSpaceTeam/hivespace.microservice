using System.Linq.Expressions;
using HiveSpace.Domain.Shared.Specifications;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate.Specifications;

public class ProductInCategorySpecification(int categoryId) : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
        => product => product.Categories.Any(c => c.CategoryId == categoryId);
}
