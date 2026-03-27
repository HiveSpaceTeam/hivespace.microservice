using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class SkuVariant : ValueObject
    {
        public string VariantName { get; private set; }
        public string Value { get; private set; }

        public SkuVariant(string value)
        {
            Value = value;
        }

        public SkuVariant(string variantName, string value)
        {
            VariantName = variantName;
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
