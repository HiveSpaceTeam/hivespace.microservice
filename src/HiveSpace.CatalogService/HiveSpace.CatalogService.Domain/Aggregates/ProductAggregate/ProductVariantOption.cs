using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductVariantOption : ValueObject
    {
        public string Value { get; private set; }

        public ProductVariantOption(string value)
        {
            Value = value;
        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
