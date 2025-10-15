using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductVariantOption : ValueObject
    {
        public Guid ProductVariantId { get; private set; }
        public Guid OptionId { get; private set; }
        public string Value { get; private set; }

        public ProductVariantOption(Guid productVariantId, Guid optionId, string value)
        {
            ProductVariantId = productVariantId;
            OptionId = optionId;
            Value = value;
        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ProductVariantId; 
            yield return OptionId;
            yield return Value;
        }
    }
}
