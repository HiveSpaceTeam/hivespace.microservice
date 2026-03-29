using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductCategory(int categoryId) : ValueObject
    {
        public int CategoryId { get; private set; } = categoryId;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CategoryId;
        }
    }


}
