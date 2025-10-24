using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductCategory(Guid productId, Guid categoryId) : ValueObject
    {
        public Guid ProductId { get; private set; } = productId;

        public Guid CategoryId { get; private set; } = categoryId;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ProductId;
            yield return CategoryId;
        }
    }

}
