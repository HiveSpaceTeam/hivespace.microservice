using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductVariantOption : ValueObject
    {
        public Guid VariantId { get; private set; }
        public Guid OptionId { get; private set; }
        public string Value { get; private set; }

        [JsonConstructor]
        public ProductVariantOption(Guid variantId, Guid optionId, string value)
        {
            VariantId = variantId;
            OptionId = optionId;
            Value = value;
        }

        private ProductVariantOption() { }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
